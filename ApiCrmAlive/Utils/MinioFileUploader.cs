using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace ApiCrmAlive.Utils;

public sealed class MinioFileUploader : IFileUploader
{
    private readonly IMinioClient _minio;
    private readonly MinioOptions _options;
    private readonly ILogger<MinioFileUploader> _logger;

    private volatile bool _bucketEnsured;

    public MinioFileUploader(IOptions<MinioOptions> options, ILogger<MinioFileUploader> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("Minio:Endpoint não está configurado.");
        if (string.IsNullOrWhiteSpace(_options.AccessKey))
            throw new InvalidOperationException("Minio:AccessKey não está configurado.");
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Minio:SecretKey não está configurado.");
        if (string.IsNullOrWhiteSpace(_options.Bucket))
            throw new InvalidOperationException("Minio:Bucket não está configurado.");

        var (host, port, useSsl) = ParseEndpoint(_options.Endpoint, _options.UseSsl);

        var client = new MinioClient()
            .WithEndpoint(host, port)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (useSsl)
            client = client.WithSSL();

        _minio = client.Build();
    }

    public async Task<List<string>> UploadAsync(List<IFormFile> files)
    {
        var objectNames = new List<string>();
        if (files == null || files.Count == 0)
            return objectNames;

        await EnsureBucketAsync();

        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
                continue;

            try
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var objectName = $"uploads/{fileName}";

                await using var stream = file.OpenReadStream();
                var putArgs = new PutObjectArgs()
                    .WithBucket(_options.Bucket!)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

                await _minio.PutObjectAsync(putArgs);
                objectNames.Add(objectName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao enviar arquivo para o MinIO: {FileName}", file.FileName);
            }
        }

        return objectNames;
    }

    public async Task<List<string>> GetUrlsAsync(List<string> objectNames)
    {
        var urls = new List<string>();
        foreach (var rawValue in objectNames)
        {
            // Suporte a dados legados: se o valor já é uma URL, extrai o nome do objeto
            var objectName = ExtractObjectName(rawValue);

            if (_options.UsePresignedGetUrls)
            {
                var presignArgs = new PresignedGetObjectArgs()
                    .WithBucket(_options.Bucket!)
                    .WithObject(objectName)
                    .WithExpiry(Math.Clamp(_options.PresignedGetExpiryMinutes, 1, 7 * 24 * 60) * 60);

                urls.Add(await _minio.PresignedGetObjectAsync(presignArgs));
            }
            else
            {
                urls.Add(BuildPublicUrl(_options, objectName));
            }
        }
        return urls;
    }

    /// <summary>
    /// Se o valor é uma URL completa (dados legados), extrai o path do objeto.
    /// Ex: "http://minio:9000/vehicles/uploads/file.jpg?X-Amz-..." → "uploads/file.jpg"
    /// </summary>
    private string ExtractObjectName(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return value; // já é um nome de objeto

        // Remove o prefixo "/bucket/" do path
        var path = uri.AbsolutePath.TrimStart('/');
        var bucket = (_options.Bucket ?? string.Empty).TrimEnd('/') + "/";
        if (path.StartsWith(bucket, StringComparison.OrdinalIgnoreCase))
            path = path[bucket.Length..];

        return path;
    }

    private async Task EnsureBucketAsync()
    {
        if (_bucketEnsured)
            return;

        if (!_options.CreateBucketIfMissing)
        {
            _bucketEnsured = true;
            return;
        }

        try
        {
            var exists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.Bucket!));
            if (!exists)
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.Bucket!));
        }
        finally
        {
            _bucketEnsured = true;
        }
    }

    private static string BuildPublicUrl(MinioOptions options, string objectName)
    {
        var baseUrl = string.IsNullOrWhiteSpace(options.PublicBaseUrl) ? options.Endpoint! : options.PublicBaseUrl!;
        baseUrl = baseUrl.TrimEnd('/');
        objectName = objectName.TrimStart('/');
        return $"{baseUrl}/{options.Bucket}/{objectName}";
    }

    private static (string host, int port, bool useSsl) ParseEndpoint(string endpoint, bool useSslFallback)
    {
        endpoint = endpoint.Trim();

        // Accept full URL (http://host:9000) or host:9000.
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            var port = uri.IsDefaultPort ? (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80) : uri.Port;
            return (uri.Host, port, uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase));
        }

        // Try "host:port"
        var host = endpoint;
        var portInt = useSslFallback ? 443 : 80;
        var idx = endpoint.LastIndexOf(':');
        if (idx > 0 && idx < endpoint.Length - 1 && int.TryParse(endpoint[(idx + 1)..], out var parsedPort))
        {
            host = endpoint[..idx];
            portInt = parsedPort;
        }

        return (host, portInt, useSslFallback);
    }
}
