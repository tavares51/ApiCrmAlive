using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Leads;

public class LeadInteractionMapper
{
    public static LeadInteractionDto ToDto(LeadInteraction interaction)
        => new(interaction.Id, interaction.LeadId, interaction.Description, interaction.CreatedAt);

    public static LeadInteraction FromDto(Guid leadId, LeadInteractionCreateDto dto, Guid createdBy)
        => new()
        {
            LeadId = leadId,
            Description = dto.Description,
            CreatedBy = createdBy
        };
}
