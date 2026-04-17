namespace ApiCrmAlive.DTOs.Analytics;

public sealed class KpisDto
{
    public int TotalLeads { get; init; }
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int VehiclesInStock { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public double AverageDaysInStock { get; init; }
    public double DailyStockAverage { get; init; }
    public TopSellerDto? TopSellerOfMonth { get; init; }
    public Guid? SellerId { get; init; }

    public int Year { get; init; }
    public int Month { get; init; }
}
