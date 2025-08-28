using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class Marketplace
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid UpdatedBy { get; set; }

    public ICollection<MarketplaceConfiguration> Configurations { get; set; } = [];
}
