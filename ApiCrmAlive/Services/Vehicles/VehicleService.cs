using System.Text.Json;
using ApiCrmAlive.DTOs.Vehicles;
using ApiCrmAlive.Mappers.Vehicles;
using ApiCrmAlive.Repositories.Vehicles;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;
using Supabase;

namespace ApiCrmAlive.Services.Vehicles;

public class VehicleService(IVehicleRepository repo, IUnitOfWork uow, VehicleMapper mapper) : IVehicleService
{
    private readonly IVehicleRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;
    private readonly VehicleMapper _mapper = mapper;

    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(
        VehicleStatusEnum? status = null, string? make = null, string? model = null,
        int? yearFrom = null, int? yearTo = null, decimal? priceFrom = null, decimal? priceTo = null,
        string? search = null, CancellationToken ct = default)
    {
        var q = _repo.Query().AsNoTracking();

        if (status.HasValue) q = q.Where(v => v.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(make)) q = q.Where(v => v.Make.Contains(make.Trim(), StringComparison.CurrentCultureIgnoreCase));
        if (!string.IsNullOrWhiteSpace(model)) q = q.Where(v => v.Model.Contains(model.Trim(), StringComparison.CurrentCultureIgnoreCase));
        if (yearFrom.HasValue) q = q.Where(v => v.Year >= yearFrom.Value);
        if (yearTo.HasValue) q = q.Where(v => v.Year <= yearTo.Value);
        if (priceFrom.HasValue) q = q.Where(v => v.Price >= priceFrom.Value);
        if (priceTo.HasValue) q = q.Where(v => v.Price <= priceTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(v => v.Make.Contains(s, StringComparison.CurrentCultureIgnoreCase) ||
                             v.Model.Contains(s, StringComparison.CurrentCultureIgnoreCase) ||
                             v.Plate.Contains(s, StringComparison.CurrentCultureIgnoreCase) ||
                             v.Color.Contains(s, StringComparison.CurrentCultureIgnoreCase));
        }

        var list = await q.OrderByDescending(v => v.CreatedAt).ToListAsync(ct);
        return [.. list.Select(_mapper.ToDto)];
    }

    public async Task<VehicleDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Veículo não encontrado.");
        return _mapper.ToDto(v);
    }

    public async Task<VehicleDto?> GetByPlateAsync(string plate, CancellationToken ct = default)
    {
        var v = await _repo.GetByPlateAsync(plate, ct);
        return v is null ? null : _mapper.ToDto(v);
    }

    public async Task<VehicleDto> CreateAsync(VehicleCreateDto dto, Guid updatedBy, CancellationToken ct = default)
    {
        var normalizedPlate = dto.Plate.Trim().ToUpperInvariant();
        if (await _repo.PlateExistsAsync(normalizedPlate, ct))
            throw new InvalidOperationException("Placa já cadastrada.");

        var entity = _mapper.FromCreateDto(dto, updatedBy);

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.ToDto(entity);
    }

    public async Task<VehicleDto> UpdateAsync(Guid id, VehicleDto dto, Guid updatedBy, CancellationToken ct = default)
    {
        var v = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Veículo não encontrado.");

        // Se placa enviada e mudou, valida unicidade
        if (!string.IsNullOrWhiteSpace(dto.Plate))
        {
            var newPlate = dto.Plate.Trim().ToUpperInvariant();
            if (!string.Equals(newPlate, v.Plate, StringComparison.OrdinalIgnoreCase) &&
                await _repo.PlateExistsAsync(newPlate, ct))
                throw new InvalidOperationException("Placa já cadastrada para outro veículo.");
        }

        // Atualiza os campos do veículo com base no VehicleDto
        v.Make = dto.Make;
        v.Model = dto.Model;
        v.Year = dto.Year;
        v.Price = dto.Price;
        v.Plate = dto.Plate.Trim().ToUpperInvariant();
        v.Color = dto.Color;
        v.Status = dto.Status;
        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = updatedBy;
        v.PhotosJson = JsonSerializer.Serialize(dto.Photos);

        _repo.Update(v);
        await _uow.SaveChangesAsync(ct);

        return _mapper.ToDto(v);
    }

    public async Task UpdateStatusAsync(Guid id, VehicleStatusEnum status, Guid updatedBy, CancellationToken ct = default)
    {
        var v = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Veículo não encontrado.");
        v.Status = status;
        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = updatedBy;

        _repo.Update(v);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Veículo não encontrado.");
        _repo.Remove(v);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<string>> GetPhotosAsync(Guid id, CancellationToken ct = default)
    {
        var vehicle = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Veículo não encontrado.");
        
        // Retorna as fotos diretamente da propriedade Photos
        return vehicle.Photos ?? [];
    }

}