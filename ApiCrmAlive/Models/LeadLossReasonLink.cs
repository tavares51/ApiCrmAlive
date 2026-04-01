using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public sealed class LeadLossReasonLink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }

    [Required]
    public int LossReasonId { get; set; }
    public LeadLossReason? LossReason { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

