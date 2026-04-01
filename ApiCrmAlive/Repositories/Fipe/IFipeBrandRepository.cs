using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Fipe;

public interface IFipeBrandRepository
{
    IQueryable<FipeBrand> Query();
    Task<FipeBrand?> GetByCodeAsync(int brandCode, CancellationToken ct = default);
    Task<bool> ExistsAsync(int brandCode, CancellationToken ct = default);
    Task AddAsync(FipeBrand entity, CancellationToken ct = default);
    void Update(FipeBrand entity);
    void Remove(FipeBrand entity);
}

