using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Models;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    public int YearModel { get; set; }

    [Required]
    [MaxLength(10)]
    public string Plate { get; set; } = string.Empty;

    [MaxLength(2)]
    public string? State { get; set; }

    [Required]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

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

    [Required]
    public FuelEnum Fuel { get; set; } = FuelEnum.flex;

    [Required]
    public TransmissionEnum Transmission { get; set; } = TransmissionEnum.manual;

    public SteeringEnum Steering { get; set; } = SteeringEnum.eletrica;

    public CategoryEnum Category { get; set; } = CategoryEnum.outros;

    [Required]
    public int Mileage { get; set; }

    public int EntryMileage { get; set; }

    public int? Renavam { get; set; }

    [Required]
    public decimal Price { get; set; }

    public decimal? CostPrice { get; set; }

    public VehicleStatusEnum Status { get; set; } = VehicleStatusEnum.Disponivel;

    [Required]
    public DateTime EntryDate { get; set; }

    public string? Description { get; set; }

    public List<string>? Features { get; set; }

    public string? PhotosJson { get; set; }

    [NotMapped]
    public List<string> Photos
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PhotosJson))
                return [];
            return JsonSerializer.Deserialize<List<string>>(PhotosJson) ?? [];
        }
        set
        {
            PhotosJson = JsonSerializer.Serialize(value);
        }
    }

    public Guid? PreviousOwnerId { get; set; }
    public Customer? PreviousOwner { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UpdatedBy { get; set; }

    public string? ModelDesc { get; set; }

    public string? Version { get; set; }

}
