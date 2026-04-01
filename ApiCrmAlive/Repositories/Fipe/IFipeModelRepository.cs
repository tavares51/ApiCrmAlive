using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Fipe;

public interface IFipeModelRepository
{
    IQueryable<FipeModel> Query();
    Task<FipeModel?> GetByCodesAsync(int brandCode, int modelCode, CancellationToken ct = default);
    Task<IReadOnlyList<FipeModel>> ListByBrandAsync(int brandCode, CancellationToken ct = default);
    Task<bool> ExistsAsync(int brandCode, int modelCode, CancellationToken ct = default);
    Task<int?> GetMaxModelCodeAsync(int brandCode, CancellationToken ct = default);
    Task AddAsync(FipeModel entity, CancellationToken ct = default);
    void Update(FipeModel entity);
    void Remove(FipeModel entity);
}

