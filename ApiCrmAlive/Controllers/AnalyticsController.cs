using ApiCrmAlive.DTOs.Analytics;
using ApiCrmAlive.Services.Analytics;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
public sealed class AnalyticsController(IAnalyticsService service) : ControllerBase
{
    private readonly IAnalyticsService _service = service;

    // KPIs
    [HttpGet("kpis")]
    [SwaggerOperation(Summary = "KPIs do dashboard", Description = "Retorna KPIs agregados (leads, clientes, estoque, receita mensal e dias médios em estoque).")]
    [SwaggerResponse(200, "KPIs retornados com sucesso", typeof(KpisDto))]
    public async Task<IActionResult> GetKpis(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int activeCustomerDays = 365,
        CancellationToken ct = default)
        => Ok(await _service.GetKpisAsync(year, month, activeCustomerDays, ct));

    // Charts
    [HttpGet("funnel")]
    [SwaggerOperation(Summary = "Funil de vendas", Description = "Contagem de leads por status (para funil).")]
    [SwaggerResponse(200, "Dados do funil retornados com sucesso", typeof(IEnumerable<FunnelStageDto>))]
    public async Task<IActionResult> GetFunnel([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetSalesFunnelAsync(from, to, ct));

    [HttpGet("leads-by-seller")]
    [SwaggerOperation(Summary = "Leads por vendedor", Description = "Agrupa leads por vendedor.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<LeadsBySellerDto>))]
    public async Task<IActionResult> GetLeadsBySeller([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetLeadsBySellerAsync(from, to, ct));

    [HttpGet("top-sellers")]
    [SwaggerOperation(Summary = "Vendedor do mes (Top 5)", Description = "Top vendedores do mes por receita (default top 5).")]
    [SwaggerResponse(200, "Ranking retornado com sucesso", typeof(IEnumerable<TopSellerDto>))]
    public async Task<IActionResult> GetTopSellersOfMonth(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int take = 5,
        CancellationToken ct = default)
        => Ok(await _service.GetTopSellersOfMonthAsync(year, month, take, ct));

    [HttpGet("conversion-by-portal")]
    [SwaggerOperation(Summary = "Conversao por portal", Description = "Taxa de conversao por fonte do lead (Lead.Source).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<ConversionByPortalDto>))]
    public async Task<IActionResult> GetConversionByPortal([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetConversionByPortalAsync(from, to, ct));

    [HttpGet("conversion-by-seller")]
    [SwaggerOperation(Summary = "Taxa de conversao por vendedor", Description = "Taxa de conversao (leads atribuídos vs leads com venda).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<ConversionBySellerDto>))]
    public async Task<IActionResult> GetConversionBySeller([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetConversionBySellerAsync(from, to, ct));

    [HttpGet("lead-conversion-summary")]
    [SwaggerOperation(Summary = "Conversao de leads (resumo)", Description = "Total de leads, leads convertidos e taxa de conversao.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(LeadConversionSummaryDto))]
    public async Task<IActionResult> GetLeadConversionSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetLeadConversionSummaryAsync(from, to, ct));

    [HttpGet("monthly-sales")]
    [SwaggerOperation(Summary = "Vendas mensais", Description = "Serie mensal de vendas (quantidade e receita).")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<MonthlySalesPointDto>))]
    public async Task<IActionResult> GetMonthlySales([FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await _service.GetMonthlySalesAsync(months, ct));

    [HttpGet("vehicles-by-status")]
    [SwaggerOperation(Summary = "Veiculos por status", Description = "Quantidade de veiculos agrupados por status.")]
    [SwaggerResponse(200, "Dados retornados com sucesso", typeof(IEnumerable<VehiclesByStatusDto>))]
    public async Task<IActionResult> GetVehiclesByStatus(CancellationToken ct)
        => Ok(await _service.GetVehiclesByStatusAsync(ct));
}

