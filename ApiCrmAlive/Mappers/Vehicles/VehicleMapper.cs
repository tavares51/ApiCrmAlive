using ApiCrmAlive.DTOs.Vehicles;
using ApiCrmAlive.Utils;         
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Vehicles;

public class VehicleMapper()
{
    public VehicleDto ToDto(Vehicle v) => new()
    {
        Id = v.Id,
        Make = v.Make,
        Model = v.Model,
        Year = v.Year,
        Plate = v.Plate,
        Color = v.Color,
        Fuel = v.Fuel,
        Transmission = v.Transmission,
        Mileage = v.Mileage,
        Price = v.Price,
        CostPrice = v.CostPrice,
        Status = v.Status,                
        EntryDate = v.EntryDate,
        Description = v.Description,
        Features = v.Features ?? [],
        Photos = v.Photos ?? [], // converte para lista vazia se for nulo
        // garante URL pública mesmo que banco tenha guardado path
        PreviousOwnerId = v.PreviousOwnerId,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt,
        UpdatedBy = v.UpdatedBy
    };

    public Vehicle FromCreateDto(VehicleCreateDto dto, Guid updatedBy) => new()
    {
        Id = Guid.NewGuid(),
        Make = dto.Make.Trim(),
        Model = dto.Model.Trim(),
        Year = dto.Year,
        Plate = NormalizePlate(dto.Plate),
        Color = dto.Color.Trim(),
        Fuel = dto.Fuel,                    
        Transmission = dto.Transmission,     
        Mileage = dto.Mileage,
        Price = dto.Price,
        CostPrice = dto.CostPrice,
        Status = dto.Status ?? VehicleStatusEnum.Disponivel,
        EntryDate = dto.EntryDate.Date,
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        Features = dto.Features?.Where(NotEmpty).Select(s => s!.Trim()).ToList(),
        // aceita path OU URL e persiste como URL pública
        PreviousOwnerId = dto.PreviousOwnerId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        UpdatedBy = updatedBy
    };

    public void ApplyUpdate(Vehicle v, VehicleUpdateDto dto, Guid updatedBy)
    {
        if (!string.IsNullOrWhiteSpace(dto.Make)) v.Make = dto.Make.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Model)) v.Model = dto.Model.Trim();
        if (dto.Year.HasValue) v.Year = dto.Year.Value;

        if (!string.IsNullOrWhiteSpace(dto.Plate)) v.Plate = NormalizePlate(dto.Plate);
        if (!string.IsNullOrWhiteSpace(dto.Color)) v.Color = dto.Color.Trim();

        if (dto.Mileage.HasValue) v.Mileage = dto.Mileage.Value;
        if (dto.Price.HasValue) v.Price = dto.Price.Value;
        if (dto.CostPrice.HasValue) v.CostPrice = dto.CostPrice;

        if (dto.Status.HasValue) v.Status = dto.Status.Value;
        if (dto.EntryDate.HasValue) v.EntryDate = dto.EntryDate.Value.Date;

        if (dto.Description is not null)
            v.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (dto.Features is not null)
            v.Features = [.. dto.Features.Where(NotEmpty).Select(s => s!.Trim())];

        if (dto.PreviousOwnerId.HasValue) v.PreviousOwnerId = dto.PreviousOwnerId;

        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = updatedBy;
    }

    private static string NormalizePlate(string plate) => plate.Trim().ToUpperInvariant();
    private static bool NotEmpty(string? s) => !string.IsNullOrWhiteSpace(s);
}
