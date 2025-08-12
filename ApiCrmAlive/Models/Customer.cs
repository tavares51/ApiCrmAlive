using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ApiCrmAlive.Models
{
    public class Customer
    {
       
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Document { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Email { get; set; }

        public JsonDocument? Address { get; set; } 

        public DateTime? BirthDate { get; set; }

        public string? Occupation { get; set; }

        public decimal? MonthlyIncome { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        public int TotalPurchases { get; set; } = 0;

        public decimal TotalSpent { get; set; } = 0;

        public string CustomerType { get; set; } = "pessoa_fisica"; 

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid UpdatedBy { get; set; }
       
    }
}
