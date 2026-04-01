using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.DTOs.LeadLossReasons;

public sealed record LeadLossReasonUpdateDto(
    [Required] string Name,
    string? Description
);

