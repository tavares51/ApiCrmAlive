using ApiCrmAlive.Utils;
using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Vehicles;

public class VehicleUpdateDto
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }

    public string? Plate { get; set; }
    public string? Color { get; set; }
    public FuelEnum? Fuel { get; set; }
    public TransmissionEnum? Transmission { get; set; }

    public int? Mileage { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }

    public VehicleStatusEnum? Status { get; set; }
    public DateTime? EntryDate { get; set; }

    public string? Description { get; set; }
    public List<string>? Features { get; set; }           
    public List<string>? Photos { get; set; }
    public List<string>? Images { get; set; } // compat: alguns clients enviam "images"
    public Guid? PreviousOwnerId { get; set; }

    public int? YearModel { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    [MaxLength(50)]
    public string? ColorIntern { get; set; }

    [MaxLength(10)]
    public string? Power { get; set; }

    public int? Doors { get; set; }
    public int? Seats { get; set; }
    public int? Speed { get; set; }
    public decimal? Engine { get; set; }

    public bool? ApprovedInjunction { get; set; }
    public string? DescInjuntion { get; set; }

    [MaxLength(17)]
    public string? Chassis { get; set; }

    public SteeringEnum? Steering { get; set; }
    public CategoryEnum? Category { get; set; }

    public int? EntryMileage { get; set; }

    [MaxLength(11)]
    public string? Renavam { get; set; }

    public string? ModelDesc { get; set; }
    public string? Version { get; set; }
}
