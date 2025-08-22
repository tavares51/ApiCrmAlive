namespace ApiCrmAlive.DTOs.Leads;

public record LeadCreateDto(
    string Name,
    string Phone,
    string? Email,
    string Source,
    Guid? VehicleInterestId,
    string? VehicleInterestDescription,
    decimal? BudgetMin,
    decimal? BudgetMax,
    bool FinancingNeeded,
    string? Notes
);
