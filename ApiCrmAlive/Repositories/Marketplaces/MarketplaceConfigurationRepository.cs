using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Marketplaces;

public class MarketplaceConfigurationRepository(AppDbContext ctx) : Repository<MarketplaceConfiguration>(ctx), IMarketplaceConfigurationRepository
{
    public  Task<MarketplaceConfiguration?> GetByMarketplaceIdAsync(Guid marketplaceId, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(x => x.MarketplaceId == marketplaceId, ct);
}
