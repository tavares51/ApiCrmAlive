namespace ApiCrmAlive.DTOs.Analytics;

public sealed class ConversionByPortalDto
{
    public string Source { get; init; } = string.Empty;
    public int LeadsCount { get; init; }
    public int ConvertedLeadsCount { get; init; }
    public double ConversionRate { get; init; }
}

