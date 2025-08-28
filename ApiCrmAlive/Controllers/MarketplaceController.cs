using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Services.Marketplaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketplacesController(IMarketplaceService service) : ControllerBase
{
    private readonly IMarketplaceService _service = service;

    // 🔹 GET ALL
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar todos os marketplaces",
        Description = "Retorna a lista de todos os marketplaces disponíveis no sistema."
    )]
    [SwaggerResponse(200, "Lista de marketplaces", typeof(IEnumerable<MarketplaceDto>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _service.GetAllAsync(ct);
        return Ok(list);
    }

    // 🔹 GET BY ID
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Obter marketplace por ID",
        Description = "Retorna os detalhes de um marketplace específico."
    )]
    [SwaggerResponse(200, "Marketplace encontrado", typeof(MarketplaceDto))]
    [SwaggerResponse(404, "Marketplace não encontrado")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var marketplace = await _service.GetByIdAsync(id, ct);
        return marketplace is not null ? Ok(marketplace) : NotFound();
    }

    // 🔹 CREATE
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar marketplace",
        Description = "Cria um novo marketplace."
    )]
    [SwaggerResponse(201, "Marketplace criado com sucesso", typeof(MarketplaceDto))]
    public async Task<IActionResult> Create([FromBody] MarketplaceCreateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid(); // injete usuário autenticado
        var result = await _service.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // 🔹 UPDATE
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar marketplace",
        Description = "Atualiza os dados de um marketplace existente."
    )]
    [SwaggerResponse(200, "Marketplace atualizado com sucesso", typeof(MarketplaceDto))]
    [SwaggerResponse(404, "Marketplace não encontrado")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MarketplaceUpdateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var result = await _service.UpdateAsync(id, dto, userId, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    // 🔹 DELETE
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover marketplace",
        Description = "Remove um marketplace do sistema."
    )]
    [SwaggerResponse(204, "Marketplace removido com sucesso")]
    [SwaggerResponse(404, "Marketplace não encontrado")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return  NoContent();
    }

    // 🔹 SYNC VEHICLE
    [HttpPost("{id:guid}/sync-vehicle/{vehicleId:guid}")]
    [SwaggerOperation(
        Summary = "Sincronizar veículo em marketplace",
        Description = "Envia um veículo específico para sincronização com o marketplace."
    )]
    [SwaggerResponse(200, "Veículo sincronizado com sucesso", typeof(MarketplaceVehicleSyncResponseDto))]
    public async Task<IActionResult> SyncVehicle(Guid id, Guid vehicleId, CancellationToken ct)
    {
        var result = await _service.SyncVehicleAsync(id, vehicleId, ct);
        return Ok(result);
    }

    // 🔹 SYNC ALL VEHICLES
    [HttpPost("sync-all-vehicles")]
    [SwaggerOperation(
        Summary = "Sincronizar todos os veículos",
        Description = "Dispara a sincronização de todos os veículos em todos os marketplaces ativos."
    )]
    [SwaggerResponse(200, "Sincronização iniciada")]
    public async Task<IActionResult> SyncAllVehicles(CancellationToken ct)
    {
        await _service.SyncAllVehiclesAsync(ct);
        return Ok(new { message = "Sincronização iniciada." });
    }

    // 🔹 SYNC STATUS
    [HttpGet("{id:guid}/sync-status")]
    [SwaggerOperation(
        Summary = "Obter status da sincronização",
        Description = "Retorna o status da última sincronização de um marketplace."
    )]
    [SwaggerResponse(200, "Status de sincronização retornado", typeof(MarketplaceSyncStatusDto))]
    public async Task<IActionResult> GetSyncStatus(Guid id, CancellationToken ct)
    {
        var result = await _service.GetSyncStatusAsync(id, ct);
        return Ok(result);
    }

    // 🔹 LOGS
    [HttpGet("{id:guid}/logs")]
    [SwaggerOperation(
        Summary = "Obter logs de integração",
        Description = "Retorna os logs de sincronização de um marketplace, com paginação."
    )]
    [SwaggerResponse(200, "Logs retornados com sucesso", typeof(PaginatedResponseDto<MarketplaceLogDto>))]
    public async Task<IActionResult> GetLogs(Guid id, [FromQuery] int page = 1, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var logs = await _service.GetLogsAsync(id, page, limit, ct);
        return Ok(logs);
    }
}
