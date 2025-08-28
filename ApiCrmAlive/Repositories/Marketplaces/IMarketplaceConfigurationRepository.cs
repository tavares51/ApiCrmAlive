using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Marketplaces;

public interface IMarketplaceConfigurationRepository : IRepository<MarketplaceConfiguration>
{
    Task<MarketplaceConfiguration?> GetByMarketplaceIdAsync(Guid marketplaceId, CancellationToken ct);
}
