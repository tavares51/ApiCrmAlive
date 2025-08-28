namespace ApiCrmAlive.DTOs.Marketplaces;

public class VehicleSyncResultDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid MarketplaceId { get; set; }
    public string ExternalId { get; set; } = null!;
    public string SyncStatus { get; set; } = null!;
    public DateTime LastSyncDate { get; set; }
}