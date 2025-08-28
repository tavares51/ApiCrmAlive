namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceConnectionTestResponseDto
{
    public string Status { get; set; } = "sucesso";
    public string Message { get; set; } = string.Empty;
    public object Details { get; set; } = new();
}
