namespace ApiCrmAlive.DTOs.Leads;

public record LeadInteractionDto(
    Guid Id,
    Guid LeadId,
    string Description,
    DateTime CreatedAt
);