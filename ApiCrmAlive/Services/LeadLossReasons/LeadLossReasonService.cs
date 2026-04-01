using ApiCrmAlive.Context;
using ApiCrmAlive.DTOs.LeadLossReasons;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.LeadLossReasons;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.LeadLossReasons;

public sealed class LeadLossReasonService(AppDbContext db, ILeadLossReasonRepository repo, IUnitOfWork uow) : ILeadLossReasonService
{
    private readonly AppDbContext _db = db;
    private readonly ILeadLossReasonRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<IReadOnlyList<LeadLossReasonDto>> GetAllAsync(Guid companyId, bool includeInactive, CancellationToken ct = default)
    {
        var q = _repo.Query().AsNoTracking().Where(x => x.CompanyId == companyId);
        if (!includeInactive) q = q.Where(x => x.IsActive);

        var list = await q.OrderBy(x => x.Name).ToListAsync(ct);
        return [.. list.Select(ToDto)];
    }

    public async Task<LeadLossReasonDto> GetByIdAsync(int id, Guid companyId, bool includeInactive, CancellationToken ct = default)
    {
        var q = _repo.Query().AsNoTracking().Where(x => x.Id == id && x.CompanyId == companyId);
        if (!includeInactive) q = q.Where(x => x.IsActive);

        var entity = await q.SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("Motivo de perda não encontrado.");
        return ToDto(entity);
    }

    public async Task<LeadLossReasonDto> CreateAsync(LeadLossReasonCreateDto dto, Guid companyId, Guid userId, CancellationToken ct = default)
    {
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório.", nameof(dto.Name));

        var exists = await _repo.Query()
            .AsNoTracking()
            .AnyAsync(x => x.CompanyId == companyId && x.Name == name, ct);

        if (exists)
            throw new InvalidOperationException("Já existe um motivo com esse nome para esta empresa.");

        var entity = new LeadLossReason
        {
            CompanyId = companyId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.LeadLossReasons.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<LeadLossReasonDto> UpdateAsync(int id, LeadLossReasonUpdateDto dto, Guid companyId, Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.Query()
            .SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Motivo de perda não encontrado.");

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório.", nameof(dto.Name));

        var duplicate = await _repo.Query()
            .AsNoTracking()
            .AnyAsync(x => x.CompanyId == companyId && x.Id != id && x.Name == name, ct);

        if (duplicate)
            throw new InvalidOperationException("Já existe um motivo com esse nome para esta empresa.");

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task ActivateAsync(int id, Guid companyId, Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.Query()
            .SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Motivo de perda não encontrado.");

        if (!entity.IsActive)
        {
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }
    }

    public async Task DeactivateAsync(int id, Guid companyId, Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.Query()
            .SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Motivo de perda não encontrado.");

        if (entity.IsActive)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }
    }

    private static LeadLossReasonDto ToDto(LeadLossReason e)
        => new(
            e.Id,
            e.CompanyId,
            e.Name,
            e.Description,
            e.IsActive,
            e.CreatedAt,
            e.UpdatedAt
        );
}

