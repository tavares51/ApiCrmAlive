using ApiCrmAlive.Utils;

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
    public List<string>? Images { get; set; }             
    public Guid? PreviousOwnerId { get; set; }
}
