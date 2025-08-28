namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceVehicleSyncResponseDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid MarketplaceId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = "enviado";
    public DateTime LastSyncDate { get; set; }
    public object SyncData { get; set; } = new();
}
