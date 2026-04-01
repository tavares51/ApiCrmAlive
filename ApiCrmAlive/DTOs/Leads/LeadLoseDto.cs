namespace ApiCrmAlive.DTOs.Leads;

public sealed record LeadLoseDto(
    IReadOnlyList<int> LossReasonIds,
    string? LossObservation
);

