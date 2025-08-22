using System.ComponentModel.DataAnnotations;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Models;

public class Lead
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    [Required]
    public string Source { get; set; } = null!;

    [Required]
    public LeadStatusEnum Status { get; set; } = LeadStatusEnum.Novo;

    public Guid? VehicleInterestId { get; set; }

    public Vehicle? VehicleInterest { get; set; }

    public string? VehicleInterestDescription { get; set; }

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public bool FinancingNeeded { get; set; } = false;

    public DateOnly? LastContactDate { get; set; }

    public DateOnly? NextFollowupDate { get; set; }

    public Guid? SellerId { get; set; }

    public User? Seller { get; set; }

    public bool HasManagerAlert { get; set; } = false;

    public string? Notes { get; set; }

    [Range(0, 100)]
    public int? ConversionProbability { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UpdatedBy { get; set; }
}
