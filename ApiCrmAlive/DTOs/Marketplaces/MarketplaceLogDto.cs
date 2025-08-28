namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceLogDto
{
    public Guid? Id { get; set; }
    public Guid? MarketplaceId { get; set; }
    public Guid? VehicleId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
}
