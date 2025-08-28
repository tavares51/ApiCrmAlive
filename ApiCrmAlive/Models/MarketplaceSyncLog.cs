using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;


public class MarketplaceSyncLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MarketplaceId { get; set; }
    public Marketplace? Marketplace { get; set; }

    public Guid? VehicleId { get; set; }

    public string Action { get; set; } = string.Empty; // create/update/delete
    public string Status { get; set; } = "sucesso";
    public int ExecutionTimeMs { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
