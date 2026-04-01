using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public sealed class FipeModel
{
    public int BrandCode { get; set; }

    public int ModelCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public FipeBrand Brand { get; set; } = default!;
}

