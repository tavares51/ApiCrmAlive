using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Leads;

public interface ILeadRepository : IRepository<Lead>
{
    Task<LeadInteraction> AddInteractionAsync(LeadInteraction interaction, CancellationToken ct = default);
    Task<List<LeadInteraction>> GetInteractionsAsync(Guid leadId, CancellationToken ct = default);
}

