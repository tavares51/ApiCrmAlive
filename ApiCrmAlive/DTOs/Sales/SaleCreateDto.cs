using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Sales;

public class SaleCreateDto
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; }
    public Guid LeadId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public decimal SalePrice { get; set; }
    public decimal DownPayment { get; set; }
    public decimal FinancingAmount { get; set; }
    public int Installments { get; set; }
    public PaymentMethodEnum? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? ContractUrl { get; set; }
}
