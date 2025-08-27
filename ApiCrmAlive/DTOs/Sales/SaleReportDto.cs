namespace ApiCrmAlive.DTOs.Sales;

public class SaleReportDto
{
    public DateTime Period { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
}
