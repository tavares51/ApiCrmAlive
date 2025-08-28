using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Marketplaces;

public class MarketplaceConfigurationRepository(AppDbContext ctx) : Repository<MarketplaceConfiguration>(ctx), IMarketplaceConfigurationRepository
{
    public Task<MarketplaceConfiguration?> GetByMarketplaceIdAsync(Guid marketplaceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
