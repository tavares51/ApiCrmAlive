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

    [Required]
    [MaxLength(10)]
    public string Plate { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    public FuelEnum Fuel { get; set; } = FuelEnum.flex;

    [Required]
    public TransmissionEnum Transmission { get; set; } = TransmissionEnum.manual;

    [Required]
    public int Mileage { get; set; }

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
}
