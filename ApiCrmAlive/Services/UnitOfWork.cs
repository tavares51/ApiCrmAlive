using ApiCrmAlive.Context;

namespace ApiCrmAlive.Services
{
    public class UnitOfWork(AppDbContext ctx) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => ctx.SaveChangesAsync(ct);
    }
}
