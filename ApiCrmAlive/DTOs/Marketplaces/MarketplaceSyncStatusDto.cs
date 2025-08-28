namespace ApiCrmAlive.DTOs.Marketplaces;

public class MarketplaceSyncStatusDto
{
    public Guid MarketplaceId { get; set; }
    public string MarketplaceName { get; set; } = string.Empty;

    public DateTime? LastSyncAt { get; set; }
    public bool IsSyncInProgress { get; set; }
    public string? LastSyncMessage { get; set; }
    public int TotalVehiclesSynced { get; set; }
    public int TotalVehiclesFailed { get; set; }
}
