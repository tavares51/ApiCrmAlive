using System.Data;
using ApiCrmAlive.Context;
using ApiCrmAlive.DTOs.Integrations;
using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Mappers.Leads;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Leads;
using ApiCrmAlive.Services.Integrations;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiCrmAlive.Services.Leads;

public class LeadService(ILeadRepository repo, IUnitOfWork uow, IEvolutionWhatsappService whatsappService, AppDbContext db) : ILeadService
{
    private readonly ILeadRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEvolutionWhatsappService _whatsappService = whatsappService;
    private readonly AppDbContext _db = db;

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

    public async Task<LeadDto> CreateAutoAssignAsync(LeadCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var normalizedPhone = PhoneUtils.NormalizeBrazilPhone(dto.Phone);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new ArgumentException("Telefone inválido.", nameof(dto.Phone));

        // Serializable + retry para evitar concorrência duplicando atribuição/telefone.
        // IMPORTANT: when EnableRetryOnFailure is configured (NpgsqlRetryingExecutionStrategy),
        // user-initiated transactions must be executed inside CreateExecutionStrategy().ExecuteAsync.
        var strategy = _db.Database.CreateExecutionStrategy();

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                    try
                    {
                        var exists = await _db.Leads
                            .AsNoTracking()
                            .AnyAsync(l => l.Phone == normalizedPhone, ct);

                        if (exists)
                            throw new InvalidOperationException("Já existe um lead com esse número de contato.");

                        var sellers = await _db.Users
                            .AsNoTracking()
                            // Role is stored as free text; be tolerant to case differences ("Vendedor" vs "vendedor").
                            .Where(u => u.IsActive && EF.Functions.ILike(u.Role, "vendedor"))
                            .OrderBy(u => u.CreatedAt)
                            .ThenBy(u => u.Id)
                            .Select(u => u.Id)
                            .ToListAsync(ct);

                        Guid? assignedSellerId = null;
                        if (sellers.Count > 0)
                        {
                            var state = await _db.SellerQueueStates.FirstOrDefaultAsync(s => s.Id == 1, ct);
                            if (state is null)
                            {
                                state = new SellerQueueState { Id = 1, LastSellerId = null, UpdatedAt = DateTime.UtcNow };
                                _db.SellerQueueStates.Add(state);
                            }

                            assignedSellerId = PickNextSeller(sellers, state.LastSellerId);
                            state.LastSellerId = assignedSellerId;
                            state.UpdatedAt = DateTime.UtcNow;
                        }

                        var normalizedDto = dto with { Phone = normalizedPhone };
                        var entity = LeadMapper.FromCreateDto(normalizedDto, userId);
                        entity.SellerId = assignedSellerId;

                        await _db.Leads.AddAsync(entity, ct);
                        await _db.SaveChangesAsync(ct);

                        // Reload with Seller included so the create response includes SellerName (not only SellerId).
                        var reloaded = await _db.Leads
                            .AsNoTracking()
                            .Include(l => l.Seller)
                            .SingleAsync(l => l.Id == entity.Id, ct);

                        await tx.CommitAsync(ct);
                        return LeadMapper.ToDto(reloaded);
                    }
                    catch
                    {
                        // Best-effort rollback; dispose would also rollback, but being explicit avoids ambiguity.
                        await tx.RollbackAsync(ct);
                        throw;
                    }
                });
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.SerializationFailure && attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pex && pex.SqlState == PostgresErrorCodes.SerializationFailure && attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), ct);
            }
        }

        throw new InvalidOperationException("Falha ao criar lead após múltiplas tentativas (concorrência).");
    }

    private static Guid PickNextSeller(IReadOnlyList<Guid> sellerIds, Guid? lastSellerId)
    {
        if (sellerIds.Count == 0)
            throw new ArgumentException("Lista de vendedores vazia.", nameof(sellerIds));

        if (lastSellerId is null)
            return sellerIds[0];

        for (var i = 0; i < sellerIds.Count; i++)
        {
            if (sellerIds[i] == lastSellerId.Value)
                return sellerIds[(i + 1) % sellerIds.Count];
        }

        // Se o último vendedor não existe mais/está inativo, reinicia do primeiro.
        return sellerIds[0];
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
            .Include(l => l.Seller)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(LeadMapper.ToDto)]; 
    }

    public async Task<LeadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await _repo.Query()
            .AsNoTracking()
            .Include(l => l.Seller)
            .SingleOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException("Lead não encontada.");

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
            .Include(l => l.Seller)
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

    public async Task<LeadDto> UpdateSellerAsync(Guid leadId, Guid sellerId, Guid userId, CancellationToken ct = default)
    {
        // Validate seller existence (active seller).
        var sellerExists = await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == sellerId && u.IsActive && EF.Functions.ILike(u.Role, "vendedor"), ct);

        if (!sellerExists)
            throw new KeyNotFoundException("Vendedor não encontrado ou inativo.");

        var lead = await _repo.GetByIdAsync(leadId, ct) ?? throw new KeyNotFoundException("Lead não encontada.");

        lead.SellerId = sellerId;
        lead.UpdatedAt = DateTime.UtcNow;
        lead.UpdatedBy = userId;

        _repo.Update(lead);
        await _uow.SaveChangesAsync(ct);

        // Reload with Seller included so the DTO includes SellerName.
        var reloaded = await _repo.Query()
            .AsNoTracking()
            .Include(l => l.Seller)
            .SingleAsync(l => l.Id == leadId, ct);

        return LeadMapper.ToDto(reloaded);
    }

    public async Task<LeadLossResultDto> LoseAsync(
        Guid leadId,
        IReadOnlyList<int> lossReasonIds,
        string? lossObservation,
        Guid companyId,
        Guid userId,
        string? userRole,
        CancellationToken ct = default)
    {
        if (lossReasonIds is null || lossReasonIds.Count == 0)
            throw new ArgumentException("Informe ao menos um motivo de perda.", nameof(lossReasonIds));

        var distinctIds = lossReasonIds.Where(x => x > 0).Distinct().ToArray();
        if (distinctIds.Length == 0)
            throw new ArgumentException("Informe ao menos um motivo de perda.", nameof(lossReasonIds));

        var leadInfo = await _db.Leads
            .AsNoTracking()
            .Where(l => l.Id == leadId)
            .Select(l => new { l.Id, l.CompanyId, l.SellerId })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Lead não encontada.");

        // Scope check: company + role.
        // If the lead doesn't have CompanyId yet, we attach it at first "lose" operation.
        if (leadInfo.CompanyId is not null && leadInfo.CompanyId != companyId)
            throw new UnauthorizedAccessException("Lead fora do escopo da sua empresa.");

        if (!RoleUtils.IsManagerOrAdmin(userRole))
        {
            // Vendedor só pode perder a própria lead.
            if (leadInfo.SellerId != userId)
                throw new UnauthorizedAccessException("Sem permissão para alterar esta lead.");
        }

        var reasons = await _db.LeadLossReasons
            .AsNoTracking()
            .Where(r => r.CompanyId == companyId && r.IsActive && distinctIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(ct);

        if (reasons.Count != distinctIds.Length)
            throw new InvalidOperationException("Um ou mais motivos não existem, não pertencem à empresa ou estão inativos.");

        var now = DateTime.UtcNow;
        var normalizedObservation = string.IsNullOrWhiteSpace(lossObservation) ? null : lossObservation.Trim();

        // Npgsql retrying execution strategy doesn't support user-initiated transactions unless
        // the whole unit-of-work runs inside CreateExecutionStrategy().ExecuteAsync(...).
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Update without relying on tracked entities; avoids EF's "expected rows affected" concurrency exception.
                var affected = await _db.Leads
                    .Where(l => l.Id == leadId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(l => l.CompanyId, l => l.CompanyId ?? companyId)
                        .SetProperty(l => l.Status, _ => LeadStatusEnum.Perdido)
                        .SetProperty(l => l.LossObservation, _ => normalizedObservation)
                        .SetProperty(l => l.UpdatedAt, _ => now)
                        .SetProperty(l => l.UpdatedBy, _ => userId), ct);

                if (affected == 0)
                    throw new KeyNotFoundException("Lead não encontada.");

                // Insert reason links idempotently: ignore duplicates caused by concurrent calls.
                const string insertSql =
                    """
                    INSERT INTO "LeadLossReasonLinks" ("Id", "LeadId", "LossReasonId", "CreatedAt")
                    VALUES (@id, @leadId, @reasonId, @createdAt)
                    ON CONFLICT ("LeadId", "LossReasonId") DO NOTHING;
                    """;

                foreach (var reasonId in distinctIds)
                {
                    var parameters = new object[]
                    {
                        new NpgsqlParameter("id", Guid.NewGuid()),
                        new NpgsqlParameter("leadId", leadId),
                        new NpgsqlParameter("reasonId", reasonId),
                        new NpgsqlParameter("createdAt", now)
                    };

                    await _db.Database.ExecuteSqlRawAsync(insertSql, parameters, ct);
                }

                await tx.CommitAsync(ct);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                // Lead (or reason) was deleted concurrently.
                await tx.RollbackAsync(ct);
                throw new KeyNotFoundException("Lead não encontada.");
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });

        var ordered = reasons.OrderBy(r => r.Name).Select(r => new LeadLossReasonRefDto(r.Id, r.Name)).ToList();
        return new LeadLossResultDto(leadId, normalizedObservation, ordered);
    }

    public async Task<LeadDto?> GetByPhoneAsync(string phone)
    {
        var normalized = PhoneUtils.NormalizeBrazilPhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        var lead = await _repo.Query().FirstOrDefaultAsync(l => l.Phone == normalized);
        return lead == null ? null : LeadMapper.ToDto(lead);
    }

    public async Task<LeadDto> CreateFromWhatsappAsync(WhatsappMessageDto message)
    {
        var dto = new LeadCreateDto(
            message.ContactName ?? "Contato WhatsApp",
            message.ContactPhone ?? string.Empty,
            null,
            "WhatsApp",
            null,
            null,
            null,
            null,
            false,
            message.Message
        );

        // Cria o lead
        var createdLead = await CreateAsync(dto, Guid.Empty);

        // Envia mensagem de saudação
        var greetingMessage = $"Olá {createdLead.Name}, obrigado por entrar em contato! Em breve retornaremos.";
        try
        {
            await _whatsappService.SendTextMessageAsync(createdLead.Phone, greetingMessage);
        }
        catch (Exception ex)
        {
            // Loga o erro, mas não impede o fluxo principal
            Console.WriteLine($"Erro ao enviar mensagem de saudação: {ex.Message}");
        }

        return createdLead;
    }
}
