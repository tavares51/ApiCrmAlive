using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Fipe;

public sealed class FipeBrandCreateDto
{
    /// <summary>Código da marca na FIPE. Opcional (se não enviar, o banco gera).</summary>
    public int? BrandCode { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
}

