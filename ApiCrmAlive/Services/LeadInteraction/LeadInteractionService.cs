using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Mappers.Leads;
using ApiCrmAlive.Mappers.Sales;
using ApiCrmAlive.Repositories;
using ApiCrmAlive.Repositories.LeadsInterations;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.LeadInteraction;

public class LeadInteractionService(ILeadInteractionRepository repo, IUnitOfWork uow) : ILeadInteractionService
{
    private readonly ILeadInteractionRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<LeadInteractionDto> CreateAsync(LeadInteractionDto dto, Guid updatedBy, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<LeadInteractionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(LeadInteractionMapper.ToDto)];
    }
}
