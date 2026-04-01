using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.LeadLossReasons;

public sealed record LeadLossReasonCreateDto(
    [Required] string Name,
    string? Description
);

