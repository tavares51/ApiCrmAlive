using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Fipe;

public sealed class FipeBrandRepository(AppDbContext ctx) : IFipeBrandRepository
{
    private readonly DbSet<FipeBrand> _db = ctx.Set<FipeBrand>();

    public IQueryable<FipeBrand> Query() => _db.AsQueryable();

    public Task<FipeBrand?> GetByCodeAsync(int brandCode, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(x => x.BrandCode == brandCode, ct);

    public Task<bool> ExistsAsync(int brandCode, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(x => x.BrandCode == brandCode, ct);

    public Task AddAsync(FipeBrand entity, CancellationToken ct = default)
        => _db.AddAsync(entity, ct).AsTask();

    public void Update(FipeBrand entity) => _db.Update(entity);
    public void Remove(FipeBrand entity) => _db.Remove(entity);
}

