using ApiCrmAlive.DTOs.Analytics;
using ApiCrmAlive.Context;
using ApiCrmAlive.Services.Analytics;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
public sealed class AnalyticsController(IAnalyticsService service, AppDbContext ctx) : ControllerBase
{
    private readonly IAnalyticsService _service = service;
    private readonly AppDbContext _ctx = ctx;

    // KPIs
    [HttpGet("kpis")]
    [SwaggerOperation(Summary = "KPIs do dashboard", Description = "Retorna KPIs agregados (leads, clientes, estoque, receita mensal e dias médios em estoque).")]
    [SwaggerResponse(200, "KPIs retornados com sucesso", typeof(KpisDto))]
    public async Task<IActionResult> GetKpis(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int activeCustomerDays = 365,
        CancellationToken ct = default)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetKpisAsync(companyId, year, month, activeCustomerDays, ct));
    }

    // Charts
    [HttpGet("funnel")]
    [SwaggerOperation(Summary = "Funil de vendas", Description = "Contagem de leads por status (para funil).")]
    [SwaggerResponse(200, "Dados do funil retornados com sucesso", typeof(IEnumerable<FunnelStageDto>))]
    public async Task<IActionResult> GetFunnel([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetSalesFunnelAsync(companyId, from, to, ct));
    }

    [HttpGet("leads-by-seller")]
    [SwaggerOperation(Summary = "Leads por vendedor", Description = "Agrupa leads por vendedor.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<LeadsBySellerDto>))]
    public async Task<IActionResult> GetLeadsBySeller([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetLeadsBySellerAsync(companyId, from, to, ct));
    }

    [HttpGet("top-sellers")]
    [SwaggerOperation(Summary = "Vendedor do mes (Top 5)", Description = "Top vendedores do mes por receita (default top 5).")]
    [SwaggerResponse(200, "Ranking retornado com sucesso", typeof(IEnumerable<TopSellerDto>))]
    public async Task<IActionResult> GetTopSellersOfMonth(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int take = 5,
        CancellationToken ct = default)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetTopSellersOfMonthAsync(companyId, year, month, take, ct));
    }

    [HttpGet("conversion-by-portal")]
    [SwaggerOperation(Summary = "Conversao por portal", Description = "Taxa de conversao por fonte do lead (Lead.Source).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<ConversionByPortalDto>))]
    public async Task<IActionResult> GetConversionByPortal([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetConversionByPortalAsync(companyId, from, to, ct));
    }

    [HttpGet("conversion-by-seller")]
    [SwaggerOperation(Summary = "Taxa de conversao por vendedor", Description = "Taxa de conversao (leads atribuídos vs leads com venda).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<ConversionBySellerDto>))]
    public async Task<IActionResult> GetConversionBySeller([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetConversionBySellerAsync(companyId, from, to, ct));
    }

    [HttpGet("lead-conversion-summary")]
    [SwaggerOperation(Summary = "Conversao de leads (resumo)", Description = "Total de leads, leads convertidos e taxa de conversao.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(LeadConversionSummaryDto))]
    public async Task<IActionResult> GetLeadConversionSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetLeadConversionSummaryAsync(companyId, from, to, ct));
    }

    [HttpGet("monthly-sales")]
    [SwaggerOperation(Summary = "Vendas mensais", Description = "Serie mensal de vendas (quantidade e receita).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<MonthlySalesPointDto>))]
    public async Task<IActionResult> GetMonthlySales([FromQuery] int months = 12, CancellationToken ct = default)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetMonthlySalesAsync(companyId, months, ct));
    }

    [HttpGet("vehicles-by-status")]
    [SwaggerOperation(Summary = "Veiculos por status", Description = "Quantidade de veiculos agrupados por status.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<VehiclesByStatusDto>))]
    public async Task<IActionResult> GetVehiclesByStatus(CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        return Ok(await _service.GetVehiclesByStatusAsync(companyId, ct));
    }
}
