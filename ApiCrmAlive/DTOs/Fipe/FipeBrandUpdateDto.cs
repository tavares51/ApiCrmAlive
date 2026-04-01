using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Fipe;

public sealed class FipeBrandUpdateDto
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
}

