using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Marketplaces;

public class MarketplaceRepository(AppDbContext ctx) : Repository<Marketplace>(ctx), IMarketplaceRepository
{

}
