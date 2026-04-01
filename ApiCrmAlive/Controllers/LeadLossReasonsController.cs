using ApiCrmAlive.DTOs.LeadLossReasons;
using ApiCrmAlive.Context;
using ApiCrmAlive.Services.LeadLossReasons;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/lead-loss-reasons")]
[Produces("application/json")]
public sealed class LeadLossReasonsController(ILeadLossReasonService service, AppDbContext ctx) : ControllerBase
{
    private readonly AppDbContext _ctx = ctx;

    [HttpGet]
    [SwaggerOperation(Summary = "Lista motivos de perda (por empresa)")]
    [SwaggerResponse(200, "Lista de motivos", typeof(IEnumerable<LeadLossReasonDto>))]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);

        // Vendedor enxerga apenas ativos (mesmo que peça includeInactive=true).
        if (!RoleUtils.IsManagerOrAdmin(User.GetRole()))
            includeInactive = false;

        var list = await service.GetAllAsync(companyId, includeInactive, ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Obtém motivo de perda por ID (por empresa)")]
    [SwaggerResponse(200, "Motivo encontrado", typeof(LeadLossReasonDto))]
    [SwaggerResponse(404, "Não encontrado")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        var includeInactive = RoleUtils.IsManagerOrAdmin(User.GetRole());
        var dto = await service.GetByIdAsync(id, companyId, includeInactive, ct);
        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "gerente,admin,administrador")]
    [SwaggerOperation(Summary = "Cria motivo de perda")]
    [SwaggerResponse(201, "Criado", typeof(LeadLossReasonDto))]
    public async Task<IActionResult> Create([FromBody] LeadLossReasonCreateDto dto, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        var userId = User.GetUserIdOrThrow();
        var created = await service.CreateAsync(dto, companyId, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "gerente,admin,administrador")]
    [SwaggerOperation(Summary = "Atualiza motivo de perda")]
    [SwaggerResponse(200, "Atualizado", typeof(LeadLossReasonDto))]
    public async Task<IActionResult> Update(int id, [FromBody] LeadLossReasonUpdateDto dto, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        var userId = User.GetUserIdOrThrow();
        var updated = await service.UpdateAsync(id, dto, companyId, userId, ct);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = "gerente,admin,administrador")]
    [SwaggerOperation(Summary = "Ativa motivo de perda")]
    [SwaggerResponse(204, "Ativado")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        var userId = User.GetUserIdOrThrow();
        await service.ActivateAsync(id, companyId, userId, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "gerente,admin,administrador")]
    [SwaggerOperation(Summary = "Inativa motivo de perda")]
    [SwaggerResponse(204, "Inativado")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var companyId = await User.GetCompanyIdOrThrowAsync(_ctx, ct);
        var userId = User.GetUserIdOrThrow();
        await service.DeactivateAsync(id, companyId, userId, ct);
        return NoContent();
    }
}
