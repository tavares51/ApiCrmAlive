namespace ApiCrmAlive.DTOs.Leads;

public sealed record LeadLossReasonRefDto(int Id, string Name);

public sealed record LeadLossResultDto(
    Guid LeadId,
    string? LossObservation,
    IReadOnlyList<LeadLossReasonRefDto> LossReasons
);

