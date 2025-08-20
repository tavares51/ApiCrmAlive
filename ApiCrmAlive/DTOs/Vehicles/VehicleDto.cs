using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Vehicles;

public class VehicleDto
{
    public Guid Id { get; set; }

    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }

    public string Plate { get; set; } = default!;
    public string Color { get; set; } = default!;
    public FuelEnum Fuel { get; set; } = FuelEnum.flex;           
    public TransmissionEnum Transmission { get; set; } = TransmissionEnum.automatico;  
    public int Mileage { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public VehicleStatusEnum Status { get; set; } = VehicleStatusEnum.Disponivel;          
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public List<string> Features { get; set; } = [];    
    public List<string> Photos { get; set; } = [];  

    public Guid? PreviousOwnerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
