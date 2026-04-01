using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ApiCrmAlive.Models;

public sealed class Company
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ normalizado (apenas digitos), com 14 caracteres.
    /// </summary>
    [Required]
    [MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Logo { get; set; }

    public string? Description { get; set; }

    // Address stored as JSONB (similar to Customer.Address)
    public JsonDocument? Address { get; set; }

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Website { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UpdatedBy { get; set; }
}

