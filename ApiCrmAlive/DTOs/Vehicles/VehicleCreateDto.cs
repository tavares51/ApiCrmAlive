using ApiCrmAlive.Utils;
using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Vehicles;

public class VehicleCreateDto
{
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public string Plate { get; set; } = default!;
    public string Color { get; set; } = default!;
    public FuelEnum Fuel { get; set; } 
    public TransmissionEnum TransmissionEnum { get; set; }
    public int Mileage { get; set; }
    public decimal Price { get; set; }
    public DateTime EntryDate { get; set; }

    public decimal? CostPrice { get; set; }
    public string? Description { get; set; }
    public List<string>? Features { get; set; }        
    public List<string>? Images { get; set; }              
    public Guid? PreviousOwnerId { get; set; }
    public VehicleStatusEnum? Status { get; set; }
    public TransmissionEnum Transmission { get; internal set; }
    public int YearModel { get; set; }

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

    public bool? ApprovedInjunction { get; set; } = false;

    public string? DescInjuntion { get; set; }

    [MaxLength(17)]
    public string? Chassis { get; set; }

    public SteeringEnum Steering { get; set; } = SteeringEnum.eletrica;

    public CategoryEnum Category { get; set; } = CategoryEnum.outros;

    public int EntryMileage { get; set; }

    public int? Renavam { get; set; }

    public string? ModelDesc { get; set; }

    public string? Version { get; set; }
}
