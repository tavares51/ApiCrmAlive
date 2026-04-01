namespace ApiCrmAlive.DTOs.Analytics;

public sealed class TopSellerDto
{
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public int SalesCount { get; init; }
    public decimal TotalRevenue { get; init; }
}

