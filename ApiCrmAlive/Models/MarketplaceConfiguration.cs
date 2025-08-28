using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class MarketplaceConfiguration
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MarketplaceId { get; set; }
    public Marketplace? Marketplace { get; set; }

    public string? ApiKey { get; set; }
    public string? AccountId { get; set; }
    public string? StoreId { get; set; }

    public string ConnectionStatus { get; set; } = "desconectado";
    public DateTime? LastTestDate { get; set; }
    public DateTime? LastSyncDate { get; set; }

    public bool AutoSyncEnabled { get; set; } = true;

    public string? ConfigurationData { get; set; } // JSON string

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid UpdatedBy { get; set; }
}
