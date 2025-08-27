using ApiCrmAlive.DTOs.Sales;
using ApiCrmAlive.Services.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;


[ApiController]
[Route("api/sales")]
[Produces("application/json")]
public class SalesController(ISaleService service) : ControllerBase
{
    private readonly ISaleService _service = service;

    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as vendas", Description = "Retorna a lista de vendas cadastradas")]
    [SwaggerResponse(200, "Lista de vendas retornada com sucesso", typeof(IEnumerable<SaleDto>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca venda por ID", Description = "Retorna uma venda específica")]
    [SwaggerResponse(200, "Venda encontrada", typeof(SaleDto))]
    [SwaggerResponse(404, "Venda não encontrada")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var sale = await _service.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma venda", Description = "Adiciona uma nova venda ao sistema")]
    [SwaggerResponse(201, "Venda criada com sucesso", typeof(SaleDto))]
    [SwaggerResponse(404, "Venda não encontrada")]
    public async Task<IActionResult> Create([FromBody] SaleCreateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid(); // TODO: substituir pelo usuário autenticado
        var created = await _service.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove uma venda", Description = "Exclui permanentemente uma venda")]
    [SwaggerResponse(204, "Venda removida com sucesso")]
    [SwaggerResponse(404, "Venda não encontrada")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza uma venda", Description = "Atualiza os dados de uma venda existente")]
    [SwaggerResponse(200, "Venda atualizada", typeof(SaleDto))]
    [SwaggerResponse(404, "Venda não encontrada")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaleUpdateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var updated = await _service.UpdateAsync(id, dto, userId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("reports")]
    [SwaggerOperation(Summary = "Gera relatórios de vendas", Description = "Retorna dados agregados sobre vendas")]
    [SwaggerResponse(200, "Relatórios gerados com sucesso", typeof(SaleReportDto))]
    [SwaggerResponse(404, "Nenhum dado de venda encontrado")]
    public async Task<IActionResult> GetReports(CancellationToken ct)
    {
        var reports = await _service.GetReportsAsync(ct);
        return reports is null ? NotFound() : Ok(reports);
    }

    [HttpGet("dashboard-stats")]
    [SwaggerOperation(Summary = "Estatísticas do dashboard de vendas", Description = "Retorna estatísticas resumidas para o dashboard")]
    [SwaggerResponse(200, "Estatísticas retornadas com sucesso", typeof(SaleDashboardStatsDto))]
    [SwaggerResponse(404, "Nenhum dado de venda encontrado")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
    {
        var stats = await _service.GetDashboardStatsAsync(ct);
        return stats is null ? NotFound() : Ok(stats);
    }

}
