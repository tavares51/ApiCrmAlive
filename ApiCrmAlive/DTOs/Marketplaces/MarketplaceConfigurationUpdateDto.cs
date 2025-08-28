using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceConfigurationUpdateDto
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string AccountId { get; set; } = string.Empty;

    [Required]
    public string StoreId { get; set; } = string.Empty;

    public bool AutoSyncEnabled { get; set; } = true;

    public string? ConfigurationData { get; set; }
}
