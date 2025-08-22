using ApiCrmAlive.DTOs.Leads;
using ApiCrmAlive.Services.Leads;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/leads")]
[Produces("application/json")]
public class LeadsController(ILeadService service) : ControllerBase
{
    private readonly ILeadService _service = service;

    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os leads", Description = "Retorna a lista de leads cadastrados")]
    [SwaggerResponse(200, "Lista de leads retornada com sucesso", typeof(IEnumerable<LeadDto>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca lead por ID", Description = "Retorna um lead específico")]
    [SwaggerResponse(200, "Lead encontrado", typeof(LeadDto))]
    [SwaggerResponse(404, "Lead não encontrado")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var lead = await _service.GetByIdAsync(id, ct);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria um lead", Description = "Adiciona um novo lead ao sistema")]
    [SwaggerResponse(201, "Lead criado com sucesso", typeof(LeadDto))]
    public async Task<IActionResult> Create([FromBody] LeadCreateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid(); // TODO: substituir pelo usuário autenticado
        var created = await _service.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza um lead", Description = "Atualiza os dados de um lead existente")]
    [SwaggerResponse(200, "Lead atualizado", typeof(LeadDto))]
    [SwaggerResponse(404, "Lead não encontrado")]
    public async Task<IActionResult> Update(Guid id, [FromBody] LeadUpdateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var updated = await _service.UpdateAsync(id, dto, userId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um lead", Description = "Exclui permanentemente um lead")]
    [SwaggerResponse(204, "Lead removido com sucesso")]
    [SwaggerResponse(404, "Lead não encontrado")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(Summary = "Atualiza status do lead", Description = "Modifica apenas o status do lead")]
    [SwaggerResponse(200, "Status atualizado", typeof(LeadDto))]
    [SwaggerResponse(404, "Lead não encontrado")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] LeadStatusUpdateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var updated = await _service.UpdateStatusAsync(id, dto.Status, userId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("kanban")]
    [SwaggerOperation(Summary = "Retorna leads em formato Kanban", Description = "Agrupa leads por status para exibição em quadro Kanban")]
    [SwaggerResponse(200, "Dados em formato Kanban retornados")]
    public async Task<IActionResult> GetKanban(CancellationToken ct)
        => Ok(await _service.GetKanbanAsync(ct));

    [HttpPost("{id:guid}/interactions")]
    [SwaggerOperation(Summary = "Adiciona interação ao lead", Description = "Cadastra uma nova interação relacionada ao lead")]
    [SwaggerResponse(200, "Interação adicionada com sucesso")]
    public async Task<IActionResult> AddInteraction(Guid id, [FromBody] LeadInteractionCreateDto dto, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var interaction = await _service.AddInteractionAsync(id, dto, userId, ct);
        return Ok(interaction);
    }

    [HttpGet("{id:guid}/interactions")]
    [SwaggerOperation(Summary = "Lista interações do lead", Description = "Retorna todas as interações de um lead específico")]
    [SwaggerResponse(200, "Interações retornadas com sucesso")]
    public async Task<IActionResult> GetInteractions(Guid id, CancellationToken ct)
        => Ok(await _service.GetInteractionsAsync(id, ct));

    [HttpPost("{id:guid}/convert")]
    [SwaggerOperation(Summary = "Converte lead", Description = "Converte um lead em cliente/negócio")]
    [SwaggerResponse(200, "Lead convertido com sucesso")]
    [SwaggerResponse(404, "Lead não encontrado")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var ok = await _service.ConvertAsync(id, userId, ct);
        return ok ? Ok(new { message = "Lead convertido." }) : NotFound();
    }
}
