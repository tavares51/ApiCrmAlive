using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public sealed class LeadLossReason
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LeadLossReasonLink> LeadLinks { get; set; } = new List<LeadLossReasonLink>();
}

