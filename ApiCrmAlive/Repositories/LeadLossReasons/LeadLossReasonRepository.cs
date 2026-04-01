using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.LeadLossReasons;

public sealed class LeadLossReasonRepository(AppDbContext ctx) : Repository<LeadLossReason>(ctx), ILeadLossReasonRepository
{
}

