using ApiCrmAlive.DTOs.Sales;
using ApiCrmAlive.Mappers.Leads;
using ApiCrmAlive.Mappers.Sales;
using ApiCrmAlive.Repositories.Sales;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Sales;

public class SaleService(ISaleRepository repo, IUnitOfWork uow) : ISaleService
{
    private readonly ISaleRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<SaleDto> CreateAsync(SaleCreateDto dto, Guid updatedBy, CancellationToken ct = default)
    {
        var entity = SaleMapper.FromCreateDto(dto, updatedBy);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return SaleMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Venda não encontrada.");
        _repo.Remove(lead);

        var affectedRows = await _uow.SaveChangesAsync(ct);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("A exclusão da venda não foi concluída com sucesso.");
        }
    }

    public async Task<IEnumerable<SaleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(SaleMapper.ToDto)];
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Registro não encontrado.");
        return SaleMapper.ToDto(sale);
    }

    public async Task<SaleDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var sales = await _repo.Query().ToListAsync(ct);

        return new SaleDashboardStatsDto
        {
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.SalePrice),
            TotalFinancing = sales.Sum(s => s.FinancingAmount),
            AverageTicket = sales.Count > 0 ? sales.Average(s => s.SalePrice) : 0,
            PendingContracts = sales.Count(s => s.Status == StatusSaleEnum.pendente),
            CompletedContracts = sales.Count(s => s.Status == StatusSaleEnum.concluido)
        };
    }

    public async Task<SaleReportDto> GetReportsAsync(CancellationToken ct = default)
    {
        var sales = await _repo.Query().ToListAsync(ct);

        return new SaleReportDto
        {
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.SalePrice),
            TotalCommission = sales.Sum(s => s.CommissionAmount)
        };
    }

    public async Task<SaleDto?> UpdateAsync(Guid id, SaleUpdateDto dto, Guid updatedBy, CancellationToken ct = default)
    {
        var sale = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Venda não encontada.");
        SaleMapper.UpdateEntity(sale, dto, updatedBy);

        _repo.Update(sale);
        await _uow.SaveChangesAsync(ct);

        return SaleMapper.ToDto(sale);
    }
}
