using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class LeadInteraction
{

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LeadId { get; set; }

    public Lead? Lead { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CreatedBy { get; set; }
}