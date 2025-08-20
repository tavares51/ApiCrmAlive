using ApiCrmAlive.DTOs.Vehicles;
using ApiCrmAlive.Utils;
using ApiCrmAlive.Services.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
public class VehiclesController(IVehicleService service) : ControllerBase
{
    /// <summary>GET /api/vehicles</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista veículos com filtros")]
    [SwaggerResponse(200, "Lista de veículos", typeof(IEnumerable<VehicleDto>))]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll(
        [FromQuery] VehicleStatusEnum? status,
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? yearFrom,
        [FromQuery] int? yearTo,
        [FromQuery] decimal? priceFrom,
        [FromQuery] decimal? priceTo,
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await service.GetAllAsync(status, make, model, yearFrom, yearTo, priceFrom, priceTo, search, ct));

    /// <summary>GET /api/vehicles/:id</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtém veículo por ID")]
    [SwaggerResponse(200, "Veículo encontrado", typeof(VehicleDto))]
    [SwaggerResponse(404, "Veículo não encontrado")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>GET /api/vehicles/by-plate/:plate</summary>
    [HttpGet("by-plate/{plate}")]
    [SwaggerOperation(Summary = "Obtém veículo por placa")]
    [SwaggerResponse(200, "Veículo encontrado", typeof(VehicleDto))]
    [SwaggerResponse(404, "Não encontrado")]
    public async Task<ActionResult<VehicleDto>> GetByPlate(string plate, CancellationToken ct)
    {
        var v = await service.GetByPlateAsync(plate, ct);
        if (v is null) return NotFound();
        return Ok(v);
    }

    /// <summary>POST /api/vehicles</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria um veículo")]
    [SwaggerResponse(201, "Criado", typeof(VehicleDto))]
    [SwaggerResponse(409, "Placa já cadastrada")]
    public async Task<ActionResult<VehicleDto>> Create([FromBody] VehicleCreateDto dto, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid(); // substitua pelo usuário autenticado
        var created = await service.CreateAsync(dto, updatedBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/vehicles/:id</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza um veículo")]
    [SwaggerResponse(200, "Atualizado", typeof(VehicleDto))]
    [SwaggerResponse(404, "Não encontrado")]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, [FromBody] VehicleDto dto, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid(); // substitua pelo usuário autenticado
        return Ok(await service.UpdateAsync(id, dto, updatedBy, ct));
    }

    /// <summary>PATCH /api/vehicles/:id/status</summary>
    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(Summary = "Altera status do veículo")]
    [SwaggerResponse(204, "Atualizado")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] VehicleStatusEnum status, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid();
        await service.UpdateStatusAsync(id, status, updatedBy, ct);
        return NoContent();
    }

    /// <summary>DELETE /api/vehicles/:id</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um veículo")]
    [SwaggerResponse(204, "Removido")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>GET /api/vehicles/:id/photos</summary>
    [HttpGet("{id:guid}/photos")]
    [SwaggerOperation(Summary = "Obtém fotos do veículo")]
    [SwaggerResponse(200, "Fotos do veículo", typeof(IEnumerable<string>))]
    [SwaggerResponse(404, "Veículo não encontrado")]
    public async Task<ActionResult<IEnumerable<string>>> GetPhotos(Guid id, CancellationToken ct)
    {
        var photos = await service.GetPhotosAsync(id, ct);
        return Ok(photos);
    }
}
