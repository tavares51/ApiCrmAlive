namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceConfigurationRequestDto
{
    public string ApiKey { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public bool AutoSyncEnabled { get; set; }
    public Dictionary<string, object>? ConfigurationData { get; set; }
}
