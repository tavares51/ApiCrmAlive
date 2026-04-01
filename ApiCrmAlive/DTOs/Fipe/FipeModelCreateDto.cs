using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.Fipe;

public sealed class FipeModelCreateDto
{
    /// <summary>Código do modelo na FIPE. Opcional (se não enviar, o serviço tenta gerar 1..N por marca).</summary>
    public int? ModelCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

