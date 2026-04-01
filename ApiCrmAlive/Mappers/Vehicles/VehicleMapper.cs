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
        UpdatedBy = v.UpdatedBy,
        YearModel = v.YearModel,
        State = v.State,
        ColorIntern = v.ColorIntern,
        Power = v.Power,
        Doors = v.Doors,
        Seats = v.Seats,
        Speed = v.Speed,
        Engine = v.Engine,
        ApprovedInjunction = v.ApprovedInjunction,
        DescInjuntion = v.DescInjuntion,
        Chassis = v.Chassis,
        Steering = v.Steering,
        Category = v.Category,
        EntryMileage = v.EntryMileage,
        Renavam = v.Renavam,
        ModelDesc = v.ModelDesc,
        Version = v.Version
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
        Status = dto.Status ?? VehicleStatusEnum.Rascunho,
        EntryDate = dto.EntryDate.Date,
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        Features = dto.Features?.Where(NotEmpty).Select(s => s!.Trim()).ToList(),
        // aceita path OU URL e persiste como URL pública
        PreviousOwnerId = dto.PreviousOwnerId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        UpdatedBy = updatedBy,
        YearModel = dto.YearModel,
        State = dto.State?.Trim(),
        ColorIntern = dto.ColorIntern?.Trim(),
        Power = dto.Power?.Trim(),
        Doors = dto.Doors,
        Seats = dto.Seats,
        Speed = dto.Speed,
        Engine = dto.Engine,
        ApprovedInjunction = dto.ApprovedInjunction,
        DescInjuntion = string.IsNullOrWhiteSpace(dto.DescInjuntion) ? null : dto.DescInjuntion.Trim(),
        Chassis = dto.Chassis?.Trim(),
        Steering = dto.Steering,
        Category = dto.Category,
        EntryMileage = dto.EntryMileage,
        Renavam = dto.Renavam,
        ModelDesc = dto.ModelDesc?.Trim(),
        Version = dto.Version?.Trim()
    };

    public void ApplyUpdate(Vehicle v, VehicleUpdateDto dto, Guid updatedBy)
    {
        if (!string.IsNullOrWhiteSpace(dto.Make)) v.Make = dto.Make.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Model)) v.Model = dto.Model.Trim();
        if (dto.Year.HasValue) v.Year = dto.Year.Value;

        if (!string.IsNullOrWhiteSpace(dto.Plate)) v.Plate = NormalizePlate(dto.Plate);
        if (!string.IsNullOrWhiteSpace(dto.Color)) v.Color = dto.Color.Trim();

        if (dto.Fuel.HasValue) v.Fuel = dto.Fuel.Value;
        if (dto.Transmission.HasValue) v.Transmission = dto.Transmission.Value;

        if (dto.Mileage.HasValue) v.Mileage = dto.Mileage.Value;
        if (dto.Price.HasValue) v.Price = dto.Price.Value;
        if (dto.CostPrice.HasValue) v.CostPrice = dto.CostPrice;

        if (dto.Status.HasValue) v.Status = dto.Status.Value;
        if (dto.EntryDate.HasValue) v.EntryDate = dto.EntryDate.Value.Date;

        if (dto.Description is not null)
            v.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (dto.Features is not null)
            v.Features = [.. dto.Features.Where(NotEmpty).Select(s => s!.Trim())];

        // Photos: prefer Photos, fall back to Images for backwards compatibility.
        var photos = dto.Photos ?? dto.Images;
        if (photos is not null)
            v.Photos = [.. photos.Where(NotEmpty).Select(s => s!.Trim())];

        if (dto.PreviousOwnerId.HasValue) v.PreviousOwnerId = dto.PreviousOwnerId;

        if (dto.YearModel.HasValue) v.YearModel = dto.YearModel.Value;

        if (dto.State is not null) v.State = string.IsNullOrWhiteSpace(dto.State) ? null : dto.State.Trim();
        if (dto.ColorIntern is not null) v.ColorIntern = string.IsNullOrWhiteSpace(dto.ColorIntern) ? null : dto.ColorIntern.Trim();
        if (dto.Power is not null) v.Power = string.IsNullOrWhiteSpace(dto.Power) ? null : dto.Power.Trim();

        if (dto.Doors.HasValue) v.Doors = dto.Doors.Value;
        if (dto.Seats.HasValue) v.Seats = dto.Seats.Value;
        if (dto.Speed.HasValue) v.Speed = dto.Speed.Value;
        if (dto.Engine.HasValue) v.Engine = dto.Engine.Value;

        if (dto.ApprovedInjunction.HasValue) v.ApprovedInjunction = dto.ApprovedInjunction.Value;
        if (dto.DescInjuntion is not null) v.DescInjuntion = string.IsNullOrWhiteSpace(dto.DescInjuntion) ? null : dto.DescInjuntion.Trim();

        if (dto.Chassis is not null) v.Chassis = string.IsNullOrWhiteSpace(dto.Chassis) ? null : dto.Chassis.Trim();

        if (dto.Steering.HasValue) v.Steering = dto.Steering.Value;
        if (dto.Category.HasValue) v.Category = dto.Category.Value;

        if (dto.EntryMileage.HasValue) v.EntryMileage = dto.EntryMileage.Value;
        if (dto.Renavam is not null) v.Renavam = string.IsNullOrWhiteSpace(dto.Renavam) ? null : dto.Renavam.Trim();
        if (dto.ModelDesc is not null) v.ModelDesc = string.IsNullOrWhiteSpace(dto.ModelDesc) ? null : dto.ModelDesc.Trim();
        if (dto.Version is not null) v.Version = string.IsNullOrWhiteSpace(dto.Version) ? null : dto.Version.Trim();

        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = updatedBy;
    }

    private static string NormalizePlate(string plate) => plate.Trim().ToUpperInvariant();
    private static bool NotEmpty(string? s) => !string.IsNullOrWhiteSpace(s);
}
