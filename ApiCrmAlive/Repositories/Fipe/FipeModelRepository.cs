using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Fipe;

public sealed class FipeModelRepository(AppDbContext ctx) : IFipeModelRepository
{
    private readonly DbSet<FipeModel> _db = ctx.Set<FipeModel>();

    public IQueryable<FipeModel> Query() => _db.AsQueryable();

    public Task<FipeModel?> GetByCodesAsync(int brandCode, int modelCode, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(x => x.BrandCode == brandCode && x.ModelCode == modelCode, ct);

    public async Task<IReadOnlyList<FipeModel>> ListByBrandAsync(int brandCode, CancellationToken ct = default)
        => await _db.AsNoTracking()
            .Where(x => x.BrandCode == brandCode)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int brandCode, int modelCode, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(x => x.BrandCode == brandCode && x.ModelCode == modelCode, ct);

    public async Task<int?> GetMaxModelCodeAsync(int brandCode, CancellationToken ct = default)
        => await _db.AsNoTracking()
            .Where(x => x.BrandCode == brandCode)
            .MaxAsync(x => (int?)x.ModelCode, ct);

    public Task AddAsync(FipeModel entity, CancellationToken ct = default)
        => _db.AddAsync(entity, ct).AsTask();

    public void Update(FipeModel entity) => _db.Update(entity);
    public void Remove(FipeModel entity) => _db.Remove(entity);
}

