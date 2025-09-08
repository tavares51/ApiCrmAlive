using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApiCrmAlive.DTOs.Marketplaces.MercadoLivre;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ApiCrmAlive.Services.Marketplaces.MercadoLivre;

public class MercadoLivreAuthService(HttpClient http) : IMercadoLivreAuthService
{
    private readonly HttpClient _http = http;
    private const string TokenFile = "ml_token.json";

    private const string ClientId = "3384901064949504";
    private const string ClientSecret = "bohz1QInpx0B6W5H3CAT1LQzyKPRes3A";
    private const string RedirectUri = "https://www.google.com/";
    private const string UrlToken = "https://api.mercadolibre.com/oauth/token";

    #region Helpers

    private static void SaveToken(MlTokenDto token)
    {
        token.CreatedAt = DateTime.UtcNow;
        File.WriteAllText(TokenFile, JsonConvert.SerializeObject(token));
    }

    private static MlTokenDto? LoadToken()
    {
        if (!File.Exists(TokenFile)) return null;
        return JsonConvert.DeserializeObject<MlTokenDto>(File.ReadAllText(TokenFile));
    }

    private static bool IsTokenExpired(MlTokenDto token)
    {
        var expireTime = token.CreatedAt.AddSeconds(token.ExpiresIn);
        return DateTime.UtcNow >= expireTime;
    }

    #endregion

    public async Task<MlTokenDto> AuthorizeAsync(string code, CancellationToken ct = default)
    {
        var payload = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", ClientId },
            { "client_secret", ClientSecret },
            { "code", code },
            { "redirect_uri", RedirectUri }
        };

        var response = await _http.PostAsync(UrlToken, new FormUrlEncodedContent(payload), ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        var token = JsonConvert.DeserializeObject<MlTokenDto>(json)!;

        SaveToken(token);
        return token;
    }

    public async Task<MlTokenDto> RefreshAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        var payload = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", ClientId },
            { "client_secret", ClientSecret },
            { "refresh_token", refreshToken.Refresh_Token }
        };

        using var response = await _http.PostAsync(UrlToken, new FormUrlEncodedContent(payload), ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        var token = JsonConvert.DeserializeObject<MlTokenDto>(json)!;

        SaveToken(token);

        return token;
    }


    public async Task<MlTokenDto?> CheckTokenAsync(CancellationToken ct = default)
    {
        var token = LoadToken();
        if (token is null) return null;

        if (IsTokenExpired(token))
        {
            if (string.IsNullOrWhiteSpace(token.RefreshToken))
                throw new InvalidOperationException("Token expirado e sem refresh_token.");

            return await RefreshAsync(new RefreshToken
            {
                Refresh_Token = token.RefreshToken
            }, ct);
        }

        return token;
    }

    public async Task<JsonElement?> InsertVehicleAsync(MlPublishDto dto, CancellationToken ct = default)
    {
        const string url = "https://api.mercadolibre.com/items";

        // Carrega o conteúdo do JSON
        if (!File.Exists("load_veiculo.json"))
            throw new FileNotFoundException("Arquivo 'load_veiculo.json' não encontrado.");

        var jsonPayload = await File.ReadAllTextAsync("load_veiculo.json", ct);

        // Prepara a requisição
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dto.RefreshToken);
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Envia a requisição
        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        // Lê e retorna o conteúdo como JSON
        var responseContent = await response.Content.ReadAsStringAsync(ct);
        var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseContent);

        return jsonResponse;
    }

    public Task<MlPublishResponseDto?> UpdateVehicleAsync(string id, MlPublishDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<MlPublishResponseDto?> DeleteVehicleAsync(string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<MlPublishResponseDto?> GetVehicleAsync(string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<MlPublishResponseDto?> GetVehiclesAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateUserTest(string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}