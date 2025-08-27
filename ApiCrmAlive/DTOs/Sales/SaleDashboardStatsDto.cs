namespace ApiCrmAlive.DTOs.Sales;

public class SaleDashboardStatsDto
{
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalFinancing { get; set; }
    public decimal AverageTicket { get; set; }
    public int PendingContracts { get; set; }
    public int CompletedContracts { get; set; }
}
