using ApiCrmAlive.DTOs.Vehicles;
using ApiCrmAlive.Repositories.Customers;
using ApiCrmAlive.Utils;
using ApiCrmAlive.Services.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/vehicles")]
[Route("api/veiculo")]
[Produces("application/json")]
public class VehiclesController(IVehicleService service, ICustomerRepository customers, IFileUploader uploader) : ControllerBase
{
    private readonly ICustomerRepository _customers = customers;
    private readonly IFileUploader _uploader = uploader;

    private Guid GetActorUserIdOrThrow() => User.GetUserIdOrThrow();

    private async Task ValidatePreviousOwnerAsync(Guid? previousOwnerId, CancellationToken ct)
    {
        if (!previousOwnerId.HasValue)
            return;

        if (previousOwnerId.Value == Guid.Empty)
        {
            ModelState.AddModelError("PreviousOwnerId", "PreviousOwnerId inválido.");
            return;
        }

        var exists = await _customers.Query()
            .AsNoTracking()
            .AnyAsync(c => c.Id == previousOwnerId.Value, ct);

        if (!exists)
            ModelState.AddModelError("PreviousOwnerId", "Cliente informado em PreviousOwnerId não existe.");
    }

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
        await ValidatePreviousOwnerAsync(dto.PreviousOwnerId, ct);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetActorUserIdOrThrow();
        var created = await service.CreateAsync(dto, updatedBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>POST /api/vehicles (multipart/form-data)</summary>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Cria um veículo (form-data) com fotos opcionais")]
    [SwaggerResponse(201, "Criado", typeof(VehicleDto))]
    [SwaggerResponse(400, "Payload inválido")]
    [SwaggerResponse(409, "Placa já cadastrada")]
    public async Task<ActionResult<VehicleDto>> CreateForm(
        [FromForm] VehicleCreateDto dto,
        [FromForm] List<IFormFile>? photos,
        CancellationToken ct)
    {
        await ValidatePreviousOwnerAsync(dto.PreviousOwnerId, ct);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetActorUserIdOrThrow();
        var created = await service.CreateAsync(dto, updatedBy, ct);

        var photoFiles = (photos ?? [])
            .Where(p => p is { Length: > 0 })
            .ToList();

        if (photoFiles.Count == 0)
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        var objectNames = await _uploader.UploadAsync(photoFiles);
        if (objectNames.Count == 0)
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        var updated = await service.PatchAsync(
            created.Id,
            new VehicleUpdateDto { Photos = objectNames },
            updatedBy,
            ct);

        return CreatedAtAction(nameof(GetById), new { id = updated.Id }, updated);
    }

    /// <summary>POST /api/vehicles/with-photos</summary>
    [HttpPost("with-photos")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Cria um veículo com fotos",
        Description = "Envie o campo 'vehicle' com o JSON do VehicleCreateDto e, opcionalmente, arquivos em 'photos'.")]
    [SwaggerResponse(201, "Criado", typeof(VehicleDto))]
    [SwaggerResponse(400, "Payload inválido")]
    [SwaggerResponse(409, "Placa já cadastrada")]
    public async Task<ActionResult<VehicleDto>> CreateWithPhotos([FromForm] VehicleCreateWithPhotosFormDto form, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(form.Vehicle))
        {
            ModelState.AddModelError(nameof(form.Vehicle), "Campo 'vehicle' é obrigatório.");
            return ValidationProblem(ModelState);
        }

        VehicleCreateDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<VehicleCreateDto>(
                form.Vehicle,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            ModelState.AddModelError(nameof(form.Vehicle), $"JSON inválido: {ex.Message}");
            return ValidationProblem(ModelState);
        }

        if (dto is null)
        {
            ModelState.AddModelError(nameof(form.Vehicle), "Não foi possível desserializar o JSON do veículo.");
            return ValidationProblem(ModelState);
        }

        TryValidateModel(dto);
        await ValidatePreviousOwnerAsync(dto.PreviousOwnerId, ct);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetActorUserIdOrThrow();
        var created = await service.CreateAsync(dto, updatedBy, ct);

        var photoFiles = (form.Photos ?? [])
            .Where(p => p is { Length: > 0 })
            .ToList();

        if (photoFiles.Count == 0)
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        var objectNames = await _uploader.UploadAsync(photoFiles);
        if (objectNames.Count == 0)
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        var updated = await service.PatchAsync(
            created.Id,
            new VehicleUpdateDto { Photos = objectNames },
            updatedBy,
            ct);

        return CreatedAtAction(nameof(GetById), new { id = updated.Id }, updated);
    }

    /// <summary>PUT /api/vehicles/:id</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza um veículo")]
    [SwaggerResponse(200, "Atualizado", typeof(VehicleDto))]
    [SwaggerResponse(404, "Não encontrado")]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, [FromBody] VehiclePutDto dto, CancellationToken ct)
    {
        await ValidatePreviousOwnerAsync(dto.PreviousOwnerId, ct);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetActorUserIdOrThrow();
        return Ok(await service.UpdateAsync(id, dto, updatedBy, ct));
    }

    /// <summary>PATCH /api/vehicles/:id</summary>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza parcialmente um veículo")]
    [SwaggerResponse(200, "Atualizado", typeof(VehicleDto))]
    [SwaggerResponse(404, "Não encontrado")]
    public async Task<ActionResult<VehicleDto>> Patch(Guid id, [FromBody] VehicleUpdateDto dto, CancellationToken ct)
    {
        await ValidatePreviousOwnerAsync(dto.PreviousOwnerId, ct);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetActorUserIdOrThrow();
        return Ok(await service.PatchAsync(id, dto, updatedBy, ct));
    }

    /// <summary>PATCH /api/vehicles/:id/status</summary>
    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(Summary = "Altera status do veículo")]
    [SwaggerResponse(204, "Atualizado")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] VehicleStatusEnum status, CancellationToken ct)
    {
        var updatedBy = GetActorUserIdOrThrow();
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

}
