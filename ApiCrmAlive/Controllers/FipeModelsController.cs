using ApiCrmAlive.DTOs.Fipe;
using ApiCrmAlive.Services.Fipe;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/fipe/brands/{brandCode:int}/models")]
[Produces("application/json")]
public sealed class FipeModelsController(IFipeModelService service) : ControllerBase
{
    /// <summary>GET /api/fipe/brands/:brandCode/models</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista modelos FIPE de uma marca")]
    [SwaggerResponse(200, "Lista de modelos", typeof(IEnumerable<FipeModelDto>))]
    [SwaggerResponse(404, "Marca não encontrada")]
    public async Task<ActionResult<IEnumerable<FipeModelDto>>> GetAllByBrand(int brandCode, [FromQuery] string? search, CancellationToken ct)
        => Ok(await service.GetByBrandAsync(brandCode, search, ct));

    /// <summary>GET /api/fipe/brands/:brandCode/models/:modelCode</summary>
    [HttpGet("{modelCode:int}")]
    [SwaggerOperation(Summary = "Obtém um modelo FIPE por código")]
    [SwaggerResponse(200, "Modelo encontrado", typeof(FipeModelDto))]
    [SwaggerResponse(404, "Modelo não encontrado")]
    public async Task<ActionResult<FipeModelDto>> GetByCodes(int brandCode, int modelCode, CancellationToken ct)
        => Ok(await service.GetByCodesAsync(brandCode, modelCode, ct));

    /// <summary>POST /api/fipe/brands/:brandCode/models</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria um modelo FIPE e associa à marca")]
    [SwaggerResponse(201, "Modelo criado", typeof(FipeModelDto))]
    [SwaggerResponse(404, "Marca não encontrada")]
    public async Task<ActionResult<FipeModelDto>> Create(int brandCode, [FromBody] FipeModelCreateDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(brandCode, dto, ct);
        return CreatedAtAction(nameof(GetByCodes), new { brandCode, modelCode = created.ModelCode }, created);
    }

    /// <summary>PUT /api/fipe/brands/:brandCode/models/:modelCode</summary>
    [HttpPut("{modelCode:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza um modelo FIPE")]
    [SwaggerResponse(200, "Modelo atualizado", typeof(FipeModelDto))]
    [SwaggerResponse(404, "Modelo não encontrado")]
    public async Task<ActionResult<FipeModelDto>> Update(int brandCode, int modelCode, [FromBody] FipeModelUpdateDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(brandCode, modelCode, dto, ct));

    /// <summary>DELETE /api/fipe/brands/:brandCode/models/:modelCode</summary>
    [HttpDelete("{modelCode:int}")]
    [SwaggerOperation(Summary = "Remove um modelo FIPE")]
    [SwaggerResponse(204, "Removido")]
    [SwaggerResponse(404, "Modelo não encontrado")]
    public async Task<IActionResult> Delete(int brandCode, int modelCode, CancellationToken ct)
    {
        await service.DeleteAsync(brandCode, modelCode, ct);
        return NoContent();
    }
}

