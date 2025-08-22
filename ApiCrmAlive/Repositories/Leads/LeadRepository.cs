using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Leads;

public class LeadRepository(AppDbContext ctx) : Repository<Lead>(ctx), ILeadRepository
{
    public async Task<LeadInteraction> AddInteractionAsync(LeadInteraction interaction, CancellationToken ct = default)
    {
        await _ctx.LeadInteractions.AddAsync(interaction, ct);
        await _ctx.SaveChangesAsync(ct);
        return interaction;
    }

    public async Task<List<LeadInteraction>> GetInteractionsAsync(Guid leadId, CancellationToken ct = default)
    {
        return await _ctx.LeadInteractions
            .AsNoTracking()
            .Where(i => i.LeadId == leadId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }
}
