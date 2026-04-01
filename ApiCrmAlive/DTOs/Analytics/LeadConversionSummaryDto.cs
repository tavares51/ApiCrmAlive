namespace ApiCrmAlive.DTOs.Analytics;

public sealed class LeadConversionSummaryDto
{
    public int TotalLeads { get; init; }
    public int ConvertedLeads { get; init; }
    public double ConversionRate { get; init; }
}

