using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Fipe;

public sealed class FipeModelUpdateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

