namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceConfigurationResponseDto
{
    public Guid Id { get; set; }
    public Guid MarketplaceId { get; set; }
    public string ConnectionStatus { get; set; } = "desconectado";
    public bool AutoSyncEnabled { get; set; }
    public DateTime? LastTestDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
