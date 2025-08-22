using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Services.Leads;

public interface ILeadService
{
    Task<IReadOnlyList<LeadDto>> GetAllAsync(CancellationToken ct = default);
    Task<LeadDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<LeadDto> CreateAsync(LeadCreateDto dto, Guid userId, CancellationToken ct);
    Task<LeadDto?> UpdateAsync(Guid id, LeadUpdateDto dto, Guid userId, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<object> GetKanbanAsync(CancellationToken ct = default);
    Task<LeadInteractionDto> AddInteractionAsync(Guid leadId, LeadInteractionCreateDto dto, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<LeadInteractionDto>> GetInteractionsAsync(Guid leadId, CancellationToken ct = default);
    Task<LeadDto?> UpdateStatusAsync(Guid id, LeadStatusEnum status, Guid userId, CancellationToken ct);
    Task<bool> ConvertAsync(Guid leadId, Guid userId, CancellationToken ct = default);
}
