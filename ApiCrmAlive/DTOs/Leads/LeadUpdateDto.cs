namespace ApiCrmAlive.DTOs.Leads;

public record LeadUpdateDto(
    string? Name,
    string? Phone,
    string? Email,
    string? Source,
    Guid? VehicleInterestId,
    string? VehicleInterestDescription,
    decimal? BudgetMin,
    decimal? BudgetMax,
    bool? FinancingNeeded,
    string? Notes,
    DateOnly? NextFollowupDate,
    int? ConversionProbability
);