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
            ModelState.AddModelError("PreviousOwnerId", "Invalid PreviousOwnerId.");
            return;
        }

        var exists = await _customers.Query()
            .AsNoTracking()
            .AnyAsync(c => c.Id == previousOwnerId.Value, ct);

        if (!exists)
            ModelState.AddModelError("PreviousOwnerId", "The customer provided in PreviousOwnerId does not exist.");
    }

    /// <summary>GET /api/vehicles</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lists vehicles with filters")]
    [SwaggerResponse(200, "Vehicle list", typeof(IEnumerable<VehicleDto>))]
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
    [SwaggerOperation(Summary = "Gets a vehicle by ID")]
    [SwaggerResponse(200, "Vehicle found", typeof(VehicleDto))]
    [SwaggerResponse(404, "Vehicle not found")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>GET /api/vehicles/by-plate/:plate</summary>
    [HttpGet("by-plate/{plate}")]
    [SwaggerOperation(Summary = "Gets a vehicle by plate")]
    [SwaggerResponse(200, "Vehicle found", typeof(VehicleDto))]
    [SwaggerResponse(404, "Not found")]
    public async Task<ActionResult<VehicleDto>> GetByPlate(string plate, CancellationToken ct)
    {
        var v = await service.GetByPlateAsync(plate, ct);
        if (v is null) return NotFound();
        return Ok(v);
    }

    /// <summary>POST /api/vehicles</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Creates a vehicle")]
    [SwaggerResponse(201, "Created", typeof(VehicleDto))]
    [SwaggerResponse(409, "Plate already registered")]
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
    [SwaggerOperation(Summary = "Creates a vehicle (form-data) with optional photos")]
    [SwaggerResponse(201, "Created", typeof(VehicleDto))]
    [SwaggerResponse(400, "Invalid payload")]
    [SwaggerResponse(409, "Plate already registered")]
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
        Summary = "Creates a vehicle with photos",
        Description = "Send the 'vehicle' field with the VehicleCreateDto JSON and, optionally, files in 'photos'.")]
    [SwaggerResponse(201, "Created", typeof(VehicleDto))]
    [SwaggerResponse(400, "Invalid payload")]
    [SwaggerResponse(409, "Plate already registered")]
    public async Task<ActionResult<VehicleDto>> CreateWithPhotos([FromForm] VehicleCreateWithPhotosFormDto form, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(form.Vehicle))
        {
            ModelState.AddModelError(nameof(form.Vehicle), "The 'vehicle' field is required.");
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
            ModelState.AddModelError(nameof(form.Vehicle), $"Invalid JSON: {ex.Message}");
            return ValidationProblem(ModelState);
        }

        if (dto is null)
        {
            ModelState.AddModelError(nameof(form.Vehicle), "Could not deserialize the vehicle JSON.");
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
    [SwaggerOperation(Summary = "Updates a vehicle")]
    [SwaggerResponse(200, "Updated", typeof(VehicleDto))]
    [SwaggerResponse(404, "Not found")]
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
    [SwaggerOperation(Summary = "Partially updates a vehicle")]
    [SwaggerResponse(200, "Updated", typeof(VehicleDto))]
    [SwaggerResponse(404, "Not found")]
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
    [SwaggerOperation(Summary = "Updates the vehicle status")]
    [SwaggerResponse(204, "Updated")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] VehicleStatusEnum status, CancellationToken ct)
    {
        var updatedBy = GetActorUserIdOrThrow();
        await service.UpdateStatusAsync(id, status, updatedBy, ct);
        return NoContent();
    }

    /// <summary>DELETE /api/vehicles/:id</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Deletes a vehicle")]
    [SwaggerResponse(204, "Deleted")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

}
