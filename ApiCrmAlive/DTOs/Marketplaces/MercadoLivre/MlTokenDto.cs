using Newtonsoft.Json;

namespace ApiCrmAlive.DTOs.Marketplaces.MercadoLivre;
public class MlTokenDto
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonProperty("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


