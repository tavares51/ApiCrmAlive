using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.LeadsInterations;

public class LeadInteractionRepository(AppDbContext ctx) : Repository<LeadInteraction>(ctx), ILeadInteractionRepository
{
}
