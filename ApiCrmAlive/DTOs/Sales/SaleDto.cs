using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Sales;

public class SaleDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public Guid VehicleId { get; set; }
    public string? VehicleName { get; set; }

    public Guid SellerId { get; set; }
    public string? SellerName { get; set; }

    public Guid LeadId { get; set; }

    public DateTime SaleDate { get; set; }
    public decimal SalePrice { get; set; }
    public decimal DownPayment { get; set; }
    public decimal FinancingAmount { get; set; }
    public int Installments { get; set; }
    public PaymentMethodEnum? PaymentMethod { get; set; }
    public StatusSaleEnum Status { get; set; }

    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }

    public string? Notes { get; set; }
    public string? ContractUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
