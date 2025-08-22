using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Leads;

public record LeadDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string Source,
    LeadStatusEnum Status,
    Guid? VehicleInterestId,
    string? VehicleInterestDescription,
    decimal? BudgetMin,
    decimal? BudgetMax,
    bool FinancingNeeded,
    DateOnly? LastContactDate,
    DateOnly? NextFollowupDate,
    Guid? SellerId,
    bool HasManagerAlert,
    string? Notes,
    int? ConversionProbability,
    DateTime CreatedAt,
    DateTime UpdatedAt
);