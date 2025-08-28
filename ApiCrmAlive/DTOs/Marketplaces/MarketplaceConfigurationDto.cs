namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceConfigurationDto
{
    public Guid Id { get; set; }
    public Guid MarketplaceId { get; set; }
    public string? ApiKey { get; set; }
    public string? AccountId { get; set; }
    public string? StoreId { get; set; }
    public string ConnectionStatus { get; set; } = "desconectado";
    public DateTime? LastTestDate { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public bool AutoSyncEnabled { get; set; } = true;
    public object? ConfigurationData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
