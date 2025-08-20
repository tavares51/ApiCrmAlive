using ApiCrmAlive.Utils;

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
}
