using ApiCrmAlive.DTOs.Analytics;

namespace ApiCrmAlive.Services.Analytics;

public interface IAnalyticsService
{
    Task<KpisDto> GetKpisAsync(Guid companyId, int? year, int? month, int activeCustomerDays, CancellationToken ct = default);

    Task<IReadOnlyList<FunnelStageDto>> GetSalesFunnelAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<LeadsBySellerDto>> GetLeadsBySellerAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopSellerDto>> GetTopSellersOfMonthAsync(Guid companyId, int? year, int? month, int take, CancellationToken ct = default);

    Task<IReadOnlyList<ConversionByPortalDto>> GetConversionByPortalAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<ConversionBySellerDto>> GetConversionBySellerAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<LeadConversionSummaryDto> GetLeadConversionSummaryAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default);

    Task<IReadOnlyList<MonthlySalesPointDto>> GetMonthlySalesAsync(Guid companyId, int months, CancellationToken ct = default);
    Task<IReadOnlyList<VehiclesByStatusDto>> GetVehiclesByStatusAsync(Guid companyId, CancellationToken ct = default);
}
