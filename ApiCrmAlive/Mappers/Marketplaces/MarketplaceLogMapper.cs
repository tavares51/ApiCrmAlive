using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Marketplaces;

public static class MarketplaceLogMapper
{
    public static MarketplaceLogDto ToDto(MarketplaceSyncLog e)
    {
        return new MarketplaceLogDto
        {
            Id = e.Id,
            MarketplaceId = e.MarketplaceId,
            VehicleId = e.VehicleId,
            Action = e.Action,
            Status = e.Status,
            ExecutionTimeMs = e.ExecutionTimeMs,
            CreatedAt = e.CreatedAt
        };
    }
}
