using ApiCrmAlive.Context;
using ApiCrmAlive.DTOs.Analytics;
using ApiCrmAlive.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Analytics;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;
    private readonly TimeZoneInfo _analyticsTimeZone;

    public AnalyticsService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _analyticsTimeZone = ResolveAnalyticsTimeZone(config);
    }

    private static TimeZoneInfo ResolveAnalyticsTimeZone(IConfiguration config)
    {
        // Postgres columns are "timestamp without time zone" in this project; treat "SaleDate" month boundaries as
        // business-local wall time, not server UTC, to avoid month-shift bugs when clients send local timestamps.
        var tzId = config["Analytics:TimeZone"];
        if (string.IsNullOrWhiteSpace(tzId))
            tzId = "America/Sao_Paulo";

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }
        catch
        {
            // Windows uses different IDs. Keep a small fallback without pulling extra dependencies.
            if (string.Equals(tzId, "America/Sao_Paulo", StringComparison.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); } catch { /* ignore */ }
            }

            return TimeZoneInfo.Utc;
        }
    }

    private (int Year, int Month) GetNowYearMonthInAnalyticsTz()
    {
        var nowTz = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _analyticsTimeZone);
        return (nowTz.Year, nowTz.Month);
    }

    private static (DateTime MonthStart, DateTime NextMonthStart) GetMonthRange(int year, int month)
    {
        // Unspecified aligns best with "timestamp without time zone".
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return (monthStart, monthStart.AddMonths(1));
    }

    private IQueryable<Guid> CompanyUserIds(Guid companyId)
        => _db.Users.AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .Select(u => u.Id);

    private IQueryable<Models.Lead> CompanyLeads(Guid companyId)
    {
        var userIds = CompanyUserIds(companyId);
        // Prefer the explicit Lead.CompanyId, but keep a fallback using UpdatedBy for older rows.
        return _db.Leads.AsNoTracking()
            .Where(l => l.CompanyId == companyId || (l.CompanyId == null && userIds.Contains(l.UpdatedBy)));
    }

    public async Task<KpisDto> GetKpisAsync(Guid companyId, int? year, int? month, int activeCustomerDays, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var (nowYear, nowMonth) = GetNowYearMonthInAnalyticsTz();
        var y = year ?? nowYear;
        var m = month ?? nowMonth;
        var (monthStart, nextMonthStart) = GetMonthRange(y, m);
        var activeCutoff = nowUtc.AddDays(-Math.Abs(activeCustomerDays));

        var companyUserIds = CompanyUserIds(companyId);
        var leads = CompanyLeads(companyId);

        // IMPORTANT: A single DbContext instance cannot execute multiple operations concurrently.
        // Keep these sequential (or use separate contexts via IDbContextFactory) to avoid 409/InvalidOperationException.
        var totalLeads = await leads.CountAsync(ct);

        // No "IsActive" flag in Customer. Define "active" as recently purchased or having any purchase history.
        // Customer doesn't have CompanyId yet; scope by UpdatedBy (user belonging to company).
        var activeCustomers = await _db.Customers.AsNoTracking()
            .Where(c => companyUserIds.Contains(c.UpdatedBy))
            .CountAsync(c => (c.LastPurchaseDate != null && c.LastPurchaseDate >= activeCutoff) || c.TotalPurchases > 0, ct);

        var vehiclesInStock = await _db.Vehicles.AsNoTracking()
            .Where(v => companyUserIds.Contains(v.UpdatedBy))
            .CountAsync(v => v.Status == VehicleStatusEnum.Disponivel || v.Status == VehicleStatusEnum.Reservado, ct);

        var monthlyRevenue = await _db.Sales.AsNoTracking()
            .Where(s => companyUserIds.Contains(s.SellerId))
            .Where(s => s.SaleDate >= monthStart && s.SaleDate < nextMonthStart)
            .SumAsync(s => (decimal?)s.SalePrice, ct);

        // Compute average "days in stock" for vehicles currently in stock, based on EntryDate.
        // Kept as in-memory to avoid provider-specific date arithmetic translation issues.
        var entryDates = await _db.Vehicles.AsNoTracking()
            .Where(v => companyUserIds.Contains(v.UpdatedBy))
            .Where(v => v.Status == VehicleStatusEnum.Disponivel || v.Status == VehicleStatusEnum.Reservado)
            .Select(v => v.EntryDate)
            .ToListAsync(ct);

        var avgDays = entryDates.Count == 0
            ? 0.0
            : entryDates.Average(d => (nowUtc - DateTime.SpecifyKind(d, DateTimeKind.Utc)).TotalDays);

        return new KpisDto
        {
            TotalLeads = totalLeads,
            ActiveCustomers = activeCustomers,
            VehiclesInStock = vehiclesInStock,
            MonthlyRevenue = monthlyRevenue ?? 0m,
            AverageDaysInStock = avgDays,
            Year = y,
            Month = m
        };
    }

    public async Task<IReadOnlyList<FunnelStageDto>> GetSalesFunnelAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = CompanyLeads(companyId).AsQueryable();
        if (from.HasValue) q = q.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(l => l.CreatedAt < to.Value);

        var grouped = await q
            .GroupBy(l => l.Status)
            .Select(g => new FunnelStageDto { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Ensure stable ordering in the funnel.
        var order = new[]
        {
            LeadStatusEnum.Novo,
            LeadStatusEnum.EmNegociacao,
            LeadStatusEnum.EmAgendamentos,
            LeadStatusEnum.Convertido,
            LeadStatusEnum.Perdido
        };

        return grouped
            .OrderBy(x => Array.IndexOf(order, x.Status))
            .ToList();
    }

    public async Task<IReadOnlyList<LeadsBySellerDto>> GetLeadsBySellerAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = CompanyLeads(companyId)
            .Include(l => l.Seller)
            .AsQueryable();

        if (from.HasValue) q = q.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(l => l.CreatedAt < to.Value);

        var list = await q
            .GroupBy(l => new { l.SellerId, SellerName = l.Seller != null ? l.Seller.Name : "Sem vendedor" })
            .Select(g => new LeadsBySellerDto
            {
                SellerId = g.Key.SellerId,
                SellerName = g.Key.SellerName,
                LeadsCount = g.Count()
            })
            .OrderByDescending(x => x.LeadsCount)
            .ToListAsync(ct);

        return list;
    }

    public async Task<IReadOnlyList<TopSellerDto>> GetTopSellersOfMonthAsync(Guid companyId, int? year, int? month, int take, CancellationToken ct = default)
    {
        var (nowYear, nowMonth) = GetNowYearMonthInAnalyticsTz();
        var y = year ?? nowYear;
        var m = month ?? nowMonth;
        var (monthStart, nextMonthStart) = GetMonthRange(y, m);
        var top = Math.Clamp(take, 1, 50);
        var companyUserIds = CompanyUserIds(companyId);

        // Join Sales -> Users to provide names in one query.
        var query = from s in _db.Sales.AsNoTracking()
                    join u in _db.Users.AsNoTracking() on s.SellerId equals u.Id
                    where s.SaleDate >= monthStart && s.SaleDate < nextMonthStart
                    where companyUserIds.Contains(s.SellerId)
                    group new { s, u } by new { s.SellerId, u.Name } into g
                    orderby g.Sum(x => x.s.SalePrice) descending, g.Count() descending
                    select new TopSellerDto
                    {
                        SellerId = g.Key.SellerId,
                        SellerName = g.Key.Name,
                        SalesCount = g.Count(),
                        TotalRevenue = g.Sum(x => x.s.SalePrice)
                    };

        return await query.Take(top).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConversionByPortalDto>> GetConversionByPortalAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var companyUserIds = CompanyUserIds(companyId);
        var leads = CompanyLeads(companyId).AsQueryable();
        var sales = _db.Sales.AsNoTracking()
            .Where(s => s.LeadId != null)
            .Where(s => companyUserIds.Contains(s.SellerId))
            .AsQueryable();

        if (from.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt >= from.Value);
            sales = sales.Where(s => s.SaleDate >= from.Value);
        }

        if (to.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt < to.Value);
            sales = sales.Where(s => s.SaleDate < to.Value);
        }

        var leadsBySource = await leads
            // Defensive normalization: avoid splitting the same source by trailing/leading spaces.
            .GroupBy(l => l.Source.Trim())
            .Select(g => new { Source = g.Key, LeadsCount = g.Count() })
            .ToListAsync(ct);

        var convertedBySource = await (from s in sales
                                       join l in _db.Leads.AsNoTracking() on s.LeadId equals l.Id
                                       where l.CompanyId == companyId || (l.CompanyId == null && companyUserIds.Contains(l.UpdatedBy))
                                       select new { Source = l.Source.Trim(), LeadId = l.Id })
            .Distinct()
            .GroupBy(x => x.Source)
            .Select(g => new { Source = g.Key, Converted = g.Count() })
            .ToListAsync(ct);

        var convertedMap = convertedBySource.ToDictionary(x => x.Source, x => x.Converted);

        return leadsBySource
            .Select(x =>
            {
                var converted = convertedMap.GetValueOrDefault(x.Source, 0);
                var rate = x.LeadsCount == 0
                    ? 0.0
                    : Math.Round((double)converted / x.LeadsCount, 4, MidpointRounding.AwayFromZero);
                return new ConversionByPortalDto
                {
                    Source = x.Source,
                    LeadsCount = x.LeadsCount,
                    ConvertedLeadsCount = converted,
                    ConversionRate = rate
                };
            })
            .OrderByDescending(x => x.LeadsCount)
            .ToList();
    }

    public async Task<IReadOnlyList<ConversionBySellerDto>> GetConversionBySellerAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var companyUserIds = CompanyUserIds(companyId);
        var leads = CompanyLeads(companyId)
            .Include(l => l.Seller)
            .AsQueryable();

        var sales = _db.Sales.AsNoTracking()
            .Where(s => s.LeadId != null)
            .Where(s => companyUserIds.Contains(s.SellerId))
            .AsQueryable();

        if (from.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt >= from.Value);
            sales = sales.Where(s => s.SaleDate >= from.Value);
        }

        if (to.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt < to.Value);
            sales = sales.Where(s => s.SaleDate < to.Value);
        }

        var leadsBySeller = await leads
            .GroupBy(l => new { l.SellerId, SellerName = l.Seller != null ? l.Seller.Name : "Sem vendedor" })
            .Select(g => new { g.Key.SellerId, g.Key.SellerName, LeadsCount = g.Count() })
            .ToListAsync(ct);

        // Count distinct converted leads, attributed by lead.SellerId.
        var converted = await (from s in sales
                               join l in _db.Leads.AsNoTracking() on s.LeadId equals l.Id
                               where l.CompanyId == companyId || (l.CompanyId == null && companyUserIds.Contains(l.UpdatedBy))
                               select new { l.SellerId, l.Id })
            .Distinct()
            .GroupBy(x => x.SellerId)
            .Select(g => new { SellerId = g.Key, Converted = g.Count() })
            .ToListAsync(ct);

        var convertedNoSeller = converted.FirstOrDefault(x => x.SellerId == null)?.Converted ?? 0;
        var map = converted
            .Where(x => x.SellerId != null)
            .ToDictionary(x => x.SellerId!.Value, x => x.Converted);

        return leadsBySeller
            .Select(x =>
            {
                var conv = x.SellerId == null ? convertedNoSeller : map.GetValueOrDefault(x.SellerId.Value, 0);
                var rate = x.LeadsCount == 0
                    ? 0.0
                    : Math.Round((double)conv / x.LeadsCount, 4, MidpointRounding.AwayFromZero);
                return new ConversionBySellerDto
                {
                    SellerId = x.SellerId,
                    SellerName = x.SellerName,
                    LeadsCount = x.LeadsCount,
                    ConvertedLeadsCount = conv,
                    ConversionRate = rate
                };
            })
            .OrderByDescending(x => x.LeadsCount)
            .ToList();
    }

    public async Task<LeadConversionSummaryDto> GetLeadConversionSummaryAsync(Guid companyId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var companyUserIds = CompanyUserIds(companyId);
        var leads = CompanyLeads(companyId).AsQueryable();
        var sales = _db.Sales.AsNoTracking().Where(s => s.LeadId != null).AsQueryable();
        sales = sales.Where(s => companyUserIds.Contains(s.SellerId));

        if (from.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt >= from.Value);
            sales = sales.Where(s => s.SaleDate >= from.Value);
        }

        if (to.HasValue)
        {
            leads = leads.Where(l => l.CreatedAt < to.Value);
            sales = sales.Where(s => s.SaleDate < to.Value);
        }

        var total = await leads.CountAsync(ct);
        var converted = await (from s in sales
                               join l in _db.Leads.AsNoTracking() on s.LeadId equals l.Id
                               where l.CompanyId == companyId || (l.CompanyId == null && companyUserIds.Contains(l.UpdatedBy))
                               select s.LeadId!.Value)
            .Distinct()
            .CountAsync(ct);

        return new LeadConversionSummaryDto
        {
            TotalLeads = total,
            ConvertedLeads = converted,
            ConversionRate = total == 0
                ? 0.0
                : Math.Round((double)converted / total, 4, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<IReadOnlyList<MonthlySalesPointDto>> GetMonthlySalesAsync(Guid companyId, int months, CancellationToken ct = default)
    {
        var take = Math.Clamp(months, 1, 60);
        var (nowYear, nowMonth) = GetNowYearMonthInAnalyticsTz();
        var (thisMonthStart, nextMonthStart) = GetMonthRange(nowYear, nowMonth);
        var start = thisMonthStart.AddMonths(-(take - 1));
        var end = nextMonthStart;
        var companyUserIds = CompanyUserIds(companyId);

        var grouped = await _db.Sales.AsNoTracking()
            .Where(s => companyUserIds.Contains(s.SellerId))
            .Where(s => s.SaleDate >= start && s.SaleDate < end)
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .Select(g => new MonthlySalesPointDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                SalesCount = g.Count(),
                Revenue = g.Sum(x => x.SalePrice)
            })
            .ToListAsync(ct);

        // Fill missing months with zeros for charting.
        var map = grouped.ToDictionary(x => (x.Year, x.Month), x => x);
        var result = new List<MonthlySalesPointDto>(take);
        for (var i = 0; i < take; i++)
        {
            var d = start.AddMonths(i);
            if (!map.TryGetValue((d.Year, d.Month), out var point))
            {
                point = new MonthlySalesPointDto { Year = d.Year, Month = d.Month, SalesCount = 0, Revenue = 0m };
            }
            result.Add(point);
        }

        return result;
    }

    public async Task<IReadOnlyList<VehiclesByStatusDto>> GetVehiclesByStatusAsync(Guid companyId, CancellationToken ct = default)
    {
        var companyUserIds = CompanyUserIds(companyId);
        return await _db.Vehicles.AsNoTracking()
            .GroupBy(v => v.Status)
            .Select(g => new VehiclesByStatusDto { Status = g.Key, Count = g.Count() })
            .OrderBy(x => x.Status)
            .ToListAsync(ct);
    }
}
