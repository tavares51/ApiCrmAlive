using ApiCrmAlive.DTOs.Fipe;

namespace ApiCrmAlive.Services.Fipe;

public interface IFipeModelService
{
    Task<IReadOnlyList<FipeModelDto>> GetByBrandAsync(int brandCode, string? search = null, CancellationToken ct = default);
    Task<FipeModelDto> GetByCodesAsync(int brandCode, int modelCode, CancellationToken ct = default);
    Task<FipeModelDto> CreateAsync(int brandCode, FipeModelCreateDto input, CancellationToken ct = default);
    Task<FipeModelDto> UpdateAsync(int brandCode, int modelCode, FipeModelUpdateDto input, CancellationToken ct = default);
    Task DeleteAsync(int brandCode, int modelCode, CancellationToken ct = default);
}

