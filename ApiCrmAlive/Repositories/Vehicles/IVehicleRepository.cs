using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Vehicles;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<bool> PlateExistsAsync(string plate, CancellationToken ct = default);
    Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken ct = default);
}