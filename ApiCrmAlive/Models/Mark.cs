using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class Mark
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
}