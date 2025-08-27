using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Sales;

public class SaleUpdateDto
{
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? DownPayment { get; set; }
    public decimal? FinancingAmount { get; set; }
    public int? Installments { get; set; }
    public PaymentMethodEnum? PaymentMethod { get; set; }
    public StatusSaleEnum? Status { get; set; }
    public decimal? CommissionRate { get; set; }
    public decimal? CommissionAmount { get; set; }
    public string? Notes { get; set; }
    public string? ContractUrl { get; set; }
}
