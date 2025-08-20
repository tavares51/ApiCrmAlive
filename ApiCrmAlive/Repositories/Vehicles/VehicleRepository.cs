using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Vehicles;

public class VehicleRepository(AppDbContext context) : Repository<Vehicle>(context), IVehicleRepository
{
    private readonly AppDbContext _context = context;

    public async Task<bool> PlateExistsAsync(string plate, CancellationToken ct = default)
    {
        var p = plate.Trim().ToUpperInvariant();
        return await _ctx.Set<Vehicle>().AnyAsync(v => v.Plate == p, ct);
    }

    public Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken ct = default)
    {
        var p = plate.Trim().ToUpperInvariant();
        return _ctx.Set<Vehicle>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Plate == p, ct);
    }
}