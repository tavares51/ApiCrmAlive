using ApiCrmAlive.DTOs.Marketplaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCrmAlive.Services.Marketplaces;

public interface IMarketplaceService
{
    Task<MarketplaceDto> CreateAsync(MarketplaceCreateDto dto, Guid userId, CancellationToken ct = default);
    Task<MarketplaceDto?> UpdateAsync(Guid id, MarketplaceUpdateDto dto, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<MarketplaceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MarketplaceDto>> GetAllAsync(CancellationToken ct = default);
    Task<MarketplaceVehicleSyncResponseDto> SyncVehicleAsync(Guid id, Guid vehicleId, CancellationToken ct);
    Task<MarketplaceVehicleSyncResponseDto> SyncAllVehiclesAsync(CancellationToken ct = default);
    Task<MarketplaceSyncStatusDto> GetSyncStatusAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResponseDto<MarketplaceLogDto>> GetLogsAsync(Guid id, int page = 1, int limit = 20, CancellationToken ct = default);
}