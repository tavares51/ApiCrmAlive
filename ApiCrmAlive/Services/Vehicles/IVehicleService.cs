using ApiCrmAlive.DTOs.Vehicles;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Services.Vehicles;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(
        VehicleStatusEnum? status = null, string? make = null, string? model = null,
        int? yearFrom = null, int? yearTo = null, decimal? priceFrom = null, decimal? priceTo = null,
        string? search = null, CancellationToken ct = default);

    Task<VehicleDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VehicleDto?> GetByPlateAsync(string plate, CancellationToken ct = default);
    Task<VehicleDto> CreateAsync(VehicleCreateDto dto, Guid updatedBy, CancellationToken ct = default);
    Task<VehicleDto> UpdateAsync(Guid id, VehicleDto dto, Guid updatedBy, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, VehicleStatusEnum status, Guid updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<string>> GetPhotosAsync(Guid id, CancellationToken ct = default);
}