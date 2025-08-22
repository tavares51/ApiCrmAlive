using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Utils;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Leads;

public static class LeadMapper
{
    public static LeadDto ToDto(Lead e)
    {
        return new LeadDto(
            Id: e.Id,
            Name: e.Name,
            Phone: e.Phone,
            Email: e.Email,
            Source: e.Source,
            Status: e.Status,
            VehicleInterestId: e.VehicleInterestId,
            VehicleInterestDescription: e.VehicleInterestDescription,
            BudgetMin: e.BudgetMin,
            BudgetMax: e.BudgetMax,
            FinancingNeeded: e.FinancingNeeded,
            LastContactDate: e.LastContactDate,
            NextFollowupDate: e.NextFollowupDate,
            SellerId: e.SellerId,
            HasManagerAlert: e.HasManagerAlert,
            Notes: e.Notes,
            ConversionProbability: e.ConversionProbability,
            CreatedAt: e.CreatedAt,
            UpdatedAt: e.UpdatedAt
        );
    }

    public static Lead FromCreateDto(LeadCreateDto dto, Guid updatedBy)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Source = dto.Source.Trim(),
            Status = LeadStatusEnum.Novo,
            VehicleInterestId = dto.VehicleInterestId,
            VehicleInterestDescription = string.IsNullOrWhiteSpace(dto.VehicleInterestDescription) ? null : dto.VehicleInterestDescription.Trim(),
            BudgetMin = dto.BudgetMin,
            BudgetMax = dto.BudgetMax,
            FinancingNeeded = dto.FinancingNeeded,
            LastContactDate = null,
            NextFollowupDate = null,
            SellerId = null,
            HasManagerAlert = false,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            ConversionProbability = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
    }

    public static void UpdateEntity(Lead e, LeadUpdateDto dto, Guid updatedBy)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name)) e.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Phone)) e.Phone = dto.Phone.Trim();
        if (dto.Email is not null) e.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Source)) e.Source = dto.Source.Trim();

        if (dto.VehicleInterestId.HasValue) e.VehicleInterestId = dto.VehicleInterestId;
        if (dto.VehicleInterestDescription is not null)
            e.VehicleInterestDescription = string.IsNullOrWhiteSpace(dto.VehicleInterestDescription) ? null : dto.VehicleInterestDescription.Trim();

        if (dto.BudgetMin.HasValue) e.BudgetMin = dto.BudgetMin;
        if (dto.BudgetMax.HasValue) e.BudgetMax = dto.BudgetMax;

        if (dto.FinancingNeeded.HasValue) e.FinancingNeeded = dto.FinancingNeeded.Value;

        if (dto.Notes is not null)
            e.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        if (dto.NextFollowupDate.HasValue) e.NextFollowupDate = dto.NextFollowupDate;

        if (dto.ConversionProbability.HasValue) e.ConversionProbability = dto.ConversionProbability;

        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = updatedBy;
    }
}
