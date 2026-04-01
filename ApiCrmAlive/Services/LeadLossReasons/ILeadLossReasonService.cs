using ApiCrmAlive.DTOs.LeadLossReasons;

namespace ApiCrmAlive.Services.LeadLossReasons;

public interface ILeadLossReasonService
{
    Task<IReadOnlyList<LeadLossReasonDto>> GetAllAsync(Guid companyId, bool includeInactive, CancellationToken ct = default);
    Task<LeadLossReasonDto> GetByIdAsync(int id, Guid companyId, bool includeInactive, CancellationToken ct = default);
    Task<LeadLossReasonDto> CreateAsync(LeadLossReasonCreateDto dto, Guid companyId, Guid userId, CancellationToken ct = default);
    Task<LeadLossReasonDto> UpdateAsync(int id, LeadLossReasonUpdateDto dto, Guid companyId, Guid userId, CancellationToken ct = default);
    Task ActivateAsync(int id, Guid companyId, Guid userId, CancellationToken ct = default);
    Task DeactivateAsync(int id, Guid companyId, Guid userId, CancellationToken ct = default);
}

