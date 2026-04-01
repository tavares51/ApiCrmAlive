namespace ApiCrmAlive.DTOs.Analytics;

public sealed class ConversionBySellerDto
{
    public Guid? SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public int LeadsCount { get; init; }
    public int ConvertedLeadsCount { get; init; }
    public double ConversionRate { get; init; }
}

