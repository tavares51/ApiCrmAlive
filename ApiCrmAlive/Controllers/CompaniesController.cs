using ApiCrmAlive.DTOs.Companies;
using ApiCrmAlive.Services.Companies;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/companies")]
[Produces("application/json")]
public sealed class CompaniesController(ICompanyService service) : ControllerBase
{
    /// <summary>GET /api/companies</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista empresas (sem paginação)")]
    [SwaggerResponse(200, "Lista de empresas", typeof(IEnumerable<CompanyDto>))]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll(CancellationToken ct = default)
        => Ok(await service.GetAllAsync(ct));

    /// <summary>GET /api/companies/:id</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtém uma empresa por ID")]
    [SwaggerResponse(200, "Empresa encontrada", typeof(CompanyDto))]
    [SwaggerResponse(404, "Empresa não encontrada")]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id, CancellationToken ct = default)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>POST /api/companies</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria uma nova empresa")]
    [SwaggerResponse(201, "Empresa criada", typeof(CompanyDto))]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(409, "CNPJ já cadastrado")]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CompanyCreateDto dto, CancellationToken ct = default)
    {
        var updatedBy = Guid.NewGuid(); // troque pelo ID do usuário autenticado
        var created = await service.CreateAsync(dto, updatedBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/companies/:id</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza uma empresa")]
    [SwaggerResponse(200, "Empresa atualizada", typeof(CompanyDto))]
    [SwaggerResponse(404, "Empresa não encontrada")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] CompanyPutDto dto, CancellationToken ct = default)
    {
        var updatedBy = Guid.NewGuid(); // troque pelo ID do usuário autenticado
        return Ok(await service.UpdateAsync(id, dto, updatedBy, ct));
    }

    /// <summary>PATCH /api/companies/:id</summary>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza parcialmente uma empresa")]
    [SwaggerResponse(200, "Empresa atualizada", typeof(CompanyDto))]
    [SwaggerResponse(404, "Empresa não encontrada")]
    public async Task<ActionResult<CompanyDto>> Patch(Guid id, [FromBody] CompanyPatchDto dto, CancellationToken ct = default)
    {
        var updatedBy = Guid.NewGuid(); // troque pelo ID do usuário autenticado
        return Ok(await service.PatchAsync(id, dto, updatedBy, ct));
    }

    /// <summary>DELETE /api/companies/:id</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove uma empresa (hard delete)")]
    [SwaggerResponse(204, "Removida com sucesso")]
    [SwaggerResponse(404, "Empresa não encontrada")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}

