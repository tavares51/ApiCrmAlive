using ApiCrmAlive.Models;

namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    public List<MarketplaceConfigurationDto> Configurations { get; set; } = [];
}