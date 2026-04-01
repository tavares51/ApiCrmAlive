namespace ApiCrmAlive.DTOs.Analytics;

public sealed class MonthlySalesPointDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int SalesCount { get; init; }
    public decimal Revenue { get; init; }
}

