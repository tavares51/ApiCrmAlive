using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Mappers.Leads;
using ApiCrmAlive.Repositories.Leads;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Leads;

public class LeadService(ILeadRepository repo, IUnitOfWork uow) : ILeadService
{
    private readonly ILeadRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<LeadInteractionDto> AddInteractionAsync(Guid leadId, LeadInteractionCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var entity = LeadInteractionMapper.FromDto(leadId, dto, userId);
        await _repo.AddInteractionAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return LeadInteractionMapper.ToDto(entity);
    }

    public async Task<bool> ConvertAsync(Guid leadId, Guid userId, CancellationToken ct = default)
    {
        var lead = await _repo.GetByIdAsync(leadId, ct) ?? throw new KeyNotFoundException("Lead não encontada.");

        lead.Status = LeadStatusEnum.Convertido;
        lead.UpdatedAt = DateTime.UtcNow;
        lead.UpdatedBy = userId;
        lead.SellerId = userId;
        lead.Notes = (lead.Notes ?? "") + $"\nConvertido em {DateTime.UtcNow:dd/MM/yyyy} por {userId}";

        _repo.Update(lead);
        await _uow.SaveChangesAsync(ct);

        return true;
    }

    public async Task<LeadDto> CreateAsync(LeadCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var entity = LeadMapper.FromCreateDto(dto, userId);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return LeadMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Lead não encontrada.");
        _repo.Remove(lead);

        var affectedRows = await _uow.SaveChangesAsync(ct);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("A exclusão do lead não foi concluída com sucesso.");
        }
    }

    public async Task<IReadOnlyList<LeadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(LeadMapper.ToDto)]; 
    }

    public async Task<LeadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Lead não encontada.");
        return LeadMapper.ToDto(lead);
    }

    public async Task<IReadOnlyList<LeadInteractionDto>> GetInteractionsAsync(Guid leadId, CancellationToken ct = default)
    {
        var list = await _repo.GetInteractionsAsync(leadId, ct);
        return [.. list.Select(LeadInteractionMapper.ToDto)];
    }

    public async Task<object> GetKanbanAsync(CancellationToken ct = default)
    {
        var leads = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        var grouped = leads
            .GroupBy(l => l.Status)
            .ToDictionary(
                g => g.Key.ToString().ToLowerInvariant(),
                g => g.Select(LeadMapper.ToDto).ToList()
            );

        return new
        {
            columns = grouped
        };
    }

    public async Task<LeadDto?> UpdateAsync(Guid id, LeadUpdateDto dto, Guid userId, CancellationToken ct = default)
    {
        var lead = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Lead não encontada.");
        LeadMapper.UpdateEntity(lead, dto, userId);

        _repo.Update(lead);
        await _uow.SaveChangesAsync(ct);

        return LeadMapper.ToDto(lead);
    }

    public async Task<LeadDto?> UpdateStatusAsync(Guid id, LeadStatusEnum status, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Lead não encontada.");
        if (e is null) return null;

        e.Status = status;
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId;

        _repo.Update(e);
        await _uow.SaveChangesAsync(ct);

        return LeadMapper.ToDto(e);
    }
}
