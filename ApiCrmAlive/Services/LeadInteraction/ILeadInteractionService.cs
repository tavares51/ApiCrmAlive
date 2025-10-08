using ApiCrmAlive.DTOs.Leads;

namespace ApiCrmAlive.Services.LeadInteraction;

public interface ILeadInteractionService
{
    Task<IEnumerable<LeadInteractionDto>> GetAllAsync(CancellationToken ct = default);
    Task<LeadInteractionDto> CreateAsync(LeadInteractionDto dto, Guid updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
