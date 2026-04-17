using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ApiCrmAlive.DTOs.Vehicles;

public sealed class VehicleCreateWithPhotosFormDto
{
    [Required]
    public string Vehicle { get; set; } = string.Empty;

    public List<IFormFile>? Photos { get; set; }
}
