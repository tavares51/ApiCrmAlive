using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class Sale
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required]
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [Required]
    public Guid SellerId { get; set; }
    public User? Seller { get; set; }

    [Required]
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }

    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Precision(5, 2)]
    public decimal SalePrice { get; set; } = 0;

    [Precision(5, 2)]
    public decimal DownPayment { get; set; } = 0;

    [Precision(5, 2)]
    public decimal FinancingAmount { get; set; } = 0;

    public int Installments { get; set; } = 0;

    public PaymentMethodEnum? PaymentMethod { get; set; }

    public StatusSaleEnum Status { get; set; } = StatusSaleEnum.pendente;

    [Range(0, 100)]
    public decimal CommissionRate { get; set; } = 0;
    
    [Precision(5, 2)] 
    public decimal CommissionAmount { get; set; } = 0;

    public string? Notes { get; set; }

    public string? ContractUrl { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UpdatedBy { get; set; }
}
