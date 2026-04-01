using ApiCrmAlive.DTOs.Analytics;

namespace ApiCrmAlive.Services.Analytics;

public interface IAnalyticsService
{
    Task<KpisDto> GetKpisAsync(int? year, int? month, int activeCustomerDays, CancellationToken ct = default);

    Task<IReadOnlyList<FunnelStageDto>> GetSalesFunnelAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<LeadsBySellerDto>> GetLeadsBySellerAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopSellerDto>> GetTopSellersOfMonthAsync(int? year, int? month, int take, CancellationToken ct = default);

    Task<IReadOnlyList<ConversionByPortalDto>> GetConversionByPortalAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<ConversionBySellerDto>> GetConversionBySellerAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<LeadConversionSummaryDto> GetLeadConversionSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default);

    Task<IReadOnlyList<MonthlySalesPointDto>> GetMonthlySalesAsync(int months, CancellationToken ct = default);
    Task<IReadOnlyList<VehiclesByStatusDto>> GetVehiclesByStatusAsync(CancellationToken ct = default);
}

