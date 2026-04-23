using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ApiCrmAlive.Utils;

public sealed class SupabaseFileUploader : IFileUploader
{
    private readonly SupabaseStorageOptions _options;
    private readonly ILogger<SupabaseFileUploader> _logger;
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _bucket;
    private readonly string _serviceRoleKey;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public SupabaseFileUploader(IOptions<SupabaseStorageOptions> options, ILogger<SupabaseFileUploader> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new InvalidOperationException("Supabase:Url não está configurado.");
        if (string.IsNullOrWhiteSpace(_options.ServiceRoleKey))
            throw new InvalidOperationException("Supabase:ServiceRoleKey não está configurado.");
        if (string.IsNullOrWhiteSpace(_options.Bucket))
            throw new InvalidOperationException("Supabase:Bucket não está configurado.");

        _baseUrl = NormalizeBaseUrl(_options.Url);
        _bucket = _options.Bucket;
        _serviceRoleKey = NormalizeApiKey(_options.ServiceRoleKey);

        if (TryGetJwtRole(_serviceRoleKey, out var role) &&
            !string.Equals(role, "service_role", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Supabase:ServiceRoleKey inválida para backend. Role detectada: '{role}'. " +
                "Use a chave service_role (ou secret key de servidor) em Supabase__ServiceRoleKey.");
        }

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            ConnectTimeout = TimeSpan.FromSeconds(20),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 10
        };

        _http = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMinutes(5),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
        _http.DefaultRequestHeaders.Add("apikey", _serviceRoleKey);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
        _http.DefaultRequestHeaders.ExpectContinue = false;
    }

    public async Task<List<string>> UploadAsync(List<IFormFile> files)
    {
        var objectNames = new List<string>();
        if (files == null || files.Count == 0)
            return objectNames;

        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
                continue;

            var objectName = $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var encodedObjectName = EncodeObjectPath(objectName);
            var url = $"{_baseUrl}/storage/v1/object/{_bucket}/{encodedObjectName}";
            var bytes = await ToBytesAsync(file);
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;

            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Version = HttpVersion.Version11,
                        VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
                    };
                    request.Headers.Add("x-upsert", "true");
                    request.Headers.ConnectionClose = true;
                    request.Content = new ByteArrayContent(bytes);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    request.Content.Headers.ContentLength = bytes.Length;

                    using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        objectNames.Add(objectName);
                        break;
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Falha no upload para Supabase ({StatusCode}) tentativa {Attempt}/{MaxAttempts}: {Body}",
                        (int)response.StatusCode, attempt, maxAttempts, body);

                    if (HasRlsViolation(body))
                    {
                        _logger.LogWarning(
                            "Upload bloqueado por RLS no bucket '{Bucket}'. Verifique Supabase__ServiceRoleKey e as policies de Storage para INSERT em storage.objects.",
                            _bucket);
                    }

                    if (!IsRetryableStatus(response.StatusCode) || attempt == maxAttempts)
                        break;

                    await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt));
                }
                catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException)
                {
                    if (attempt == maxAttempts)
                    {
                        _logger.LogWarning(ex, "Erro ao enviar arquivo para Supabase Storage: {FileName}", file.FileName);
                        break;
                    }

                    _logger.LogWarning(
                        ex,
                        "Erro transitório no upload para Supabase (tentativa {Attempt}/{MaxAttempts}) arquivo: {FileName}",
                        attempt,
                        maxAttempts,
                        file.FileName);
                    await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt));
                }
            }
        }

        return objectNames;
    }

    public async Task<List<string>> GetUrlsAsync(List<string> objectNames)
    {
        var urls = new List<string>();
        if (objectNames == null || objectNames.Count == 0)
            return urls;

        foreach (var raw in objectNames)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            if (TryGetExternalAbsoluteUrl(raw, out var externalUrl))
            {
                urls.Add(externalUrl);
                continue;
            }

            var objectName = ExtractObjectName(raw);
            if (string.IsNullOrWhiteSpace(objectName))
                continue;

            // Legacy/corrupted records may contain an external URL wrapped as a Supabase object path.
            // If the extracted "object" is actually an absolute URL, return it directly.
            if (TryGetExternalAbsoluteUrl(objectName, out var externalFromObject))
            {
                urls.Add(externalFromObject);
                continue;
            }

            if (_options.UseSignedUrls)
            {
                urls.Add(await CreateSignedUrlAsync(objectName));
            }
            else
            {
                urls.Add(BuildPublicUrl(objectName));
            }
        }

        return urls;
    }

    private async Task<string> CreateSignedUrlAsync(string objectName)
    {
        var encodedObjectName = EncodeObjectPath(objectName);
        var url = $"{_baseUrl}/storage/v1/object/sign/{_bucket}/{encodedObjectName}";
        var payload = JsonSerializer.Serialize(
            new { expiresIn = Math.Clamp(_options.SignedUrlExpirySeconds, 60, 604800) },
            JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Falha ao gerar signed URL ({StatusCode}): {Body}", (int)response.StatusCode, body);
            return BuildPublicUrl(objectName);
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string? signedPath = null;
        if (root.TryGetProperty("signedURL", out var signedUrlProp))
            signedPath = signedUrlProp.GetString();
        else if (root.TryGetProperty("signedUrl", out var signedUrlCamelProp))
            signedPath = signedUrlCamelProp.GetString();

        if (string.IsNullOrWhiteSpace(signedPath))
            return BuildPublicUrl(objectName);

        if (signedPath.StartsWith("/storage/v1", StringComparison.OrdinalIgnoreCase))
            return $"{_baseUrl}{signedPath}";

        if (signedPath.StartsWith("/object/", StringComparison.OrdinalIgnoreCase))
            return $"{_baseUrl}/storage/v1{signedPath}";

        if (Uri.TryCreate(signedPath, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
             absolute.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return absolute.ToString();
        }

        return $"{_baseUrl}/storage/v1/{signedPath.TrimStart('/')}";
    }

    private string BuildPublicUrl(string objectName)
    {
        var encodedObjectName = EncodeObjectPath(objectName);
        return $"{_baseUrl}/storage/v1/object/public/{_bucket}/{encodedObjectName}";
    }

    private static string EncodeObjectPath(string objectName)
    {
        var segments = objectName.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);
        return string.Join('/', segments);
    }

    private string ExtractObjectName(string value)
    {
        var normalized = NormalizeHttpUrl(value);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            return CleanObjectName(value.TrimStart('/'));

        var path = uri.AbsolutePath.Trim('/');
        var publicPrefix = $"storage/v1/object/public/{_bucket}/";
        var signPrefix = $"storage/v1/object/sign/{_bucket}/";
        var objectPrefix = $"storage/v1/object/{_bucket}/";

        if (path.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase))
            return CleanObjectName(Uri.UnescapeDataString(path[publicPrefix.Length..]));
        if (path.StartsWith(signPrefix, StringComparison.OrdinalIgnoreCase))
            return CleanObjectName(Uri.UnescapeDataString(path[signPrefix.Length..]));
        if (path.StartsWith(objectPrefix, StringComparison.OrdinalIgnoreCase))
            return CleanObjectName(Uri.UnescapeDataString(path[objectPrefix.Length..]));

        var decodedPath = Uri.UnescapeDataString(path);
        var bucketPrefix = $"{_bucket}/";
        if (decodedPath.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
            return CleanObjectName(decodedPath[bucketPrefix.Length..]);

        var uploadsIndex = decodedPath.IndexOf("uploads/", StringComparison.OrdinalIgnoreCase);
        if (uploadsIndex >= 0)
            return CleanObjectName(decodedPath[uploadsIndex..]);

        return CleanObjectName(decodedPath);
    }

    private bool TryGetExternalAbsoluteUrl(string value, out string url)
    {
        url = string.Empty;
        foreach (var candidate in ExpandExternalUrlCandidates(value))
        {
            var normalized = NormalizeHttpUrl(candidate);
            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
                continue;

            if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                continue;

            if (uri.AbsolutePath.Contains("/storage/v1/object/", StringComparison.OrdinalIgnoreCase))
                continue;

            url = uri.ToString();
            return true;
        }

        return false;
    }

    private static IEnumerable<string> ExpandExternalUrlCandidates(string value)
    {
        var trimmed = value.Trim();
        yield return trimmed;

        if (trimmed.Contains('%'))
        {
            string decoded;
            try
            {
                decoded = Uri.UnescapeDataString(trimmed);
            }
            catch
            {
                yield break;
            }

            if (!string.Equals(decoded, trimmed, StringComparison.Ordinal))
                yield return decoded;
        }
    }

    private static string NormalizeHttpUrl(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("https:/", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + trimmed["https:/".Length..].TrimStart('/');
        }

        if (trimmed.StartsWith("http:/", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return "http://" + trimmed["http:/".Length..].TrimStart('/');
        }

        return trimmed;
    }

    private static async Task<byte[]> ToBytesAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream((int)Math.Min(file.Length, int.MaxValue));
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private static bool IsRetryableStatus(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code == 408 || code == 429 || code >= 500;
    }

    private static string CleanObjectName(string objectName)
    {
        var clean = objectName;
        var q = clean.IndexOf('?');
        if (q >= 0)
            clean = clean[..q];
        var h = clean.IndexOf('#');
        if (h >= 0)
            clean = clean[..h];
        return clean.TrimStart('/');
    }

    private static bool HasRlsViolation(string? body)
        => !string.IsNullOrWhiteSpace(body) &&
           body.Contains("row-level security policy", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeApiKey(string key)
    {
        key = key.Trim();
        const string bearerPrefix = "Bearer ";
        if (key.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            key = key[bearerPrefix.Length..].Trim();
        return key;
    }

    private static string NormalizeBaseUrl(string url)
    {
        var normalized = url.Trim().TrimEnd('/');
        const string storageV1Suffix = "/storage/v1";
        if (normalized.EndsWith(storageV1Suffix, StringComparison.OrdinalIgnoreCase))
            normalized = normalized[..^storageV1Suffix.Length];
        return normalized;
    }

    private static bool TryGetJwtRole(string key, out string? role)
    {
        role = null;
        var parts = key.Split('.');
        if (parts.Length < 2)
            return false;

        try
        {
            var payloadBytes = DecodeBase64Url(parts[1]);
            using var doc = JsonDocument.Parse(payloadBytes);
            if (doc.RootElement.TryGetProperty("role", out var roleProp))
            {
                role = roleProp.GetString();
                return !string.IsNullOrWhiteSpace(role);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] DecodeBase64Url(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        var padding = s.Length % 4;
        if (padding == 2) s += "==";
        else if (padding == 3) s += "=";
        else if (padding != 0) s += new string('=', 4 - padding);
        return Convert.FromBase64String(s);
    }
}
