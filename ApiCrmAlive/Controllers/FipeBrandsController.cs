using ApiCrmAlive.DTOs.Fipe;
using ApiCrmAlive.Services.Fipe;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/fipe/brands")]
[Produces("application/json")]
public sealed class FipeBrandsController(IFipeBrandService service) : ControllerBase
{
    /// <summary>GET /api/fipe/brands</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista marcas FIPE")]
    [SwaggerResponse(200, "Lista de marcas", typeof(IEnumerable<FipeBrandDto>))]
    public async Task<ActionResult<IEnumerable<FipeBrandDto>>> GetAll([FromQuery] string? search, CancellationToken ct)
        => Ok(await service.GetAllAsync(search, ct));

    /// <summary>GET /api/fipe/brands/:brandCode</summary>
    [HttpGet("{brandCode:int}")]
    [SwaggerOperation(Summary = "Obtém uma marca FIPE por código")]
    [SwaggerResponse(200, "Marca encontrada", typeof(FipeBrandDto))]
    [SwaggerResponse(404, "Marca não encontrada")]
    public async Task<ActionResult<FipeBrandDto>> GetByCode(int brandCode, CancellationToken ct)
        => Ok(await service.GetByCodeAsync(brandCode, ct));

    /// <summary>POST /api/fipe/brands</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria uma marca FIPE")]
    [SwaggerResponse(201, "Marca criada", typeof(FipeBrandDto))]
    public async Task<ActionResult<FipeBrandDto>> Create([FromBody] FipeBrandCreateDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetByCode), new { brandCode = created.BrandCode }, created);
    }

    /// <summary>PUT /api/fipe/brands/:brandCode</summary>
    [HttpPut("{brandCode:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza uma marca FIPE")]
    [SwaggerResponse(200, "Marca atualizada", typeof(FipeBrandDto))]
    [SwaggerResponse(404, "Marca não encontrada")]
    public async Task<ActionResult<FipeBrandDto>> Update(int brandCode, [FromBody] FipeBrandUpdateDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(brandCode, dto, ct));

    /// <summary>DELETE /api/fipe/brands/:brandCode</summary>
    [HttpDelete("{brandCode:int}")]
    [SwaggerOperation(Summary = "Remove uma marca FIPE (cascateia os modelos)")]
    [SwaggerResponse(204, "Removida")]
    [SwaggerResponse(404, "Marca não encontrada")]
    public async Task<IActionResult> Delete(int brandCode, CancellationToken ct)
    {
        await service.DeleteAsync(brandCode, ct);
        return NoContent();
    }
}

