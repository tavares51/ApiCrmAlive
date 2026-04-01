namespace ApiCrmAlive.DTOs.LeadLossReasons;

public sealed record LeadLossReasonDto(
    int Id,
    Guid CompanyId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

