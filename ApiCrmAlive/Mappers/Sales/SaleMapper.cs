using ApiCrmAlive.DTOs.Sales;
using ApiCrmAlive.Models;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Mappers.Sales;

public static class SaleMapper
{
    public static SaleDto ToDto(Sale s)
    {
        return new SaleDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            CustomerName = s.Customer?.Name,
            VehicleId = s.VehicleId,
            VehicleName = s.Vehicle?.Model,
            SellerId = s.SellerId,
            SellerName = s.Seller?.Name,
            LeadId = s.LeadId,
            SaleDate = s.SaleDate,
            SalePrice = s.SalePrice,
            DownPayment = s.DownPayment,
            FinancingAmount = s.FinancingAmount,
            Installments = s.Installments,
            PaymentMethod = s.PaymentMethod,
            Status = s.Status,
            CommissionRate = s.CommissionRate,
            CommissionAmount = s.CommissionAmount,
            Notes = s.Notes,
            ContractUrl = s.ContractUrl,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy
        };
    }

    public static Sale FromCreateDto(SaleCreateDto dto, Guid updatedBy)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            SellerId = dto.SellerId,
            LeadId = dto.LeadId,
            SaleDate = dto.SaleDate,
            SalePrice = dto.SalePrice,
            DownPayment = dto.DownPayment,
            FinancingAmount = dto.FinancingAmount,
            Installments = dto.Installments,
            PaymentMethod = dto.PaymentMethod,
            Status = StatusSaleEnum.pendente,
            CommissionRate = 0,
            CommissionAmount = 0,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            ContractUrl = string.IsNullOrWhiteSpace(dto.ContractUrl) ? null : dto.ContractUrl.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
    }

    public static void UpdateEntity(Sale entity, SaleUpdateDto dto, Guid updatedBy)
    {
        if (dto.SaleDate.HasValue)
            entity.SaleDate = dto.SaleDate.Value;

        if (dto.SalePrice.HasValue)
            entity.SalePrice = dto.SalePrice.Value;

        if (dto.DownPayment.HasValue)
            entity.DownPayment = dto.DownPayment.Value;

        if (dto.FinancingAmount.HasValue)
            entity.FinancingAmount = dto.FinancingAmount.Value;

        if (dto.Installments.HasValue)
            entity.Installments = dto.Installments.Value;

        if (dto.PaymentMethod.HasValue)
            entity.PaymentMethod = dto.PaymentMethod.Value;

        if (dto.Status.HasValue)
            entity.Status = dto.Status.Value;

        if (dto.CommissionRate.HasValue)
            entity.CommissionRate = dto.CommissionRate.Value;

        if (dto.CommissionAmount.HasValue)
            entity.CommissionAmount = dto.CommissionAmount.Value;

        if (dto.Notes is not null)
            entity.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        if (dto.ContractUrl is not null)
            entity.ContractUrl = string.IsNullOrWhiteSpace(dto.ContractUrl) ? null : dto.ContractUrl.Trim();

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}
