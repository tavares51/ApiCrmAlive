using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Marketplaces;

public class MarketplaceLogRepository(AppDbContext ctx) : Repository<MarketplaceSyncLog>(ctx), IMarketplaceLogRepository
{
    public Task<(IEnumerable<MarketplaceSyncLog> Data, int Total)> GetLogsAsync(Guid marketplaceId, int page, int limit, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
