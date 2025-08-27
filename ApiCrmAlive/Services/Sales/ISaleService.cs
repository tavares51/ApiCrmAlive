using ApiCrmAlive.DTOs.Sales;

namespace ApiCrmAlive.Services.Sales;

public interface ISaleService
{
    Task<IEnumerable<SaleDto>> GetAllAsync(CancellationToken ct = default);
    Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SaleDto> CreateAsync(SaleCreateDto dto, Guid updatedBy, CancellationToken ct = default);
    Task<SaleDto?> UpdateAsync(Guid id, SaleUpdateDto dto, Guid updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<SaleReportDto> GetReportsAsync(CancellationToken ct = default);
    Task<SaleDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
}
