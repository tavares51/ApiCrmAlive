using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Marketplaces;

public interface IMarketplaceLogRepository : IRepository<MarketplaceSyncLog>
{
    Task<(IEnumerable<MarketplaceSyncLog> Data, int Total)> GetLogsAsync(Guid marketplaceId, int page, int limit, CancellationToken ct);
}
