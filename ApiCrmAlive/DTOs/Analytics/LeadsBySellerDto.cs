namespace ApiCrmAlive.DTOs.Analytics;

public sealed class LeadsBySellerDto
{
    public Guid? SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public int LeadsCount { get; init; }
}

