using ApiCrmAlive.DTOs.Fipe;

namespace ApiCrmAlive.Services.Fipe;

public interface IFipeBrandService
{
    Task<IReadOnlyList<FipeBrandDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<FipeBrandDto> GetByCodeAsync(int brandCode, CancellationToken ct = default);
    Task<FipeBrandDto> CreateAsync(FipeBrandCreateDto input, CancellationToken ct = default);
    Task<FipeBrandDto> UpdateAsync(int brandCode, FipeBrandUpdateDto input, CancellationToken ct = default);
    Task DeleteAsync(int brandCode, CancellationToken ct = default);
}

