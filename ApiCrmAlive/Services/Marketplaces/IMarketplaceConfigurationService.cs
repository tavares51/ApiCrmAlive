using ApiCrmAlive.DTOs.Marketplaces;

namespace ApiCrmAlive.Services.Marketplaces;

public interface IMarketplaceConfigurationService
{
    Task<MarketplaceConfigurationDto> CreateAsync(Guid marketplaceId, MarketplaceConfigurationCreateDto dto, Guid userId, CancellationToken ct = default);
    Task<MarketplaceConfigurationDto> UpdateAsync(Guid marketplaceId, MarketplaceConfigurationUpdateDto dto, Guid userId, CancellationToken ct = default);
    Task<MarketplaceConfigurationDto?> GetByMarketplaceIdAsync(Guid marketplaceId, CancellationToken ct = default);

    Task<TestConnectionResultDto> TestConnectionAsync(Guid marketplaceId, CancellationToken ct = default);

    Task<VehicleSyncResultDto> SyncVehicleAsync(Guid marketplaceId, Guid vehicleId, CancellationToken ct = default);
    Task SyncAllVehiclesAsync(Guid marketplaceId, CancellationToken ct = default);

    Task<IReadOnlyList<MarketplaceLogDto>> GetLogsAsync(Guid marketplaceId, int page = 1, int limit = 20, CancellationToken ct = default);
}