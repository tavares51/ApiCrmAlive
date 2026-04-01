using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public sealed class FipeBrand
{
    [Key]
    public int BrandCode { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public List<FipeModel> Models { get; set; } = [];
}

