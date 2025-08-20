using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclePhotosController(IVehicleService service, SupabaseFileUploader uploader) : ControllerBase
    {
        private readonly IVehicleService _service = service;
        private readonly SupabaseFileUploader _uploader = uploader;

        [HttpPost("veiculo/{id}/fotos")]
        [SwaggerOperation(
            Summary = "Adiciona fotos a uma entrada de veículo",
            Description = "Envia uma ou mais fotos (form-data) para adicionar à entrada existente."
        )]
        [SwaggerResponse(200, "Fotos adicionadas com sucesso")]
        [SwaggerResponse(400, "Nenhuma foto válida foi enviada")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPhotos(Guid id, [FromForm] List<IFormFile> photos)
        {
            var vehicle = await _service.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            if (photos == null || !photos.Any(p => p.Length > 0))
                return BadRequest("Nenhuma foto válida foi enviada.");

            var newUrls = await UploadPhotosToSupabase(photos);
            if (newUrls.Count == 0)
                return BadRequest("Nenhuma foto foi adicionada com sucesso.");

            // Atualiza a propriedade Photos diretamente no modelo Vehicle
            var updatedPhotos = vehicle.Photos ?? [];
            updatedPhotos.AddRange(newUrls);
            vehicle.Photos = updatedPhotos;

            await _service.UpdateAsync(id, vehicle, Guid.NewGuid());
            return Ok(new { id = vehicle.Id, photos = vehicle.Photos });
        }

        [HttpDelete("veiculo/{id}/fotos/{fotoId}")]
        [SwaggerOperation(
            Summary = "Remove uma foto de uma entrada de veículo",
            Description = "Remove uma foto específica de uma entrada com base em seu ID ou hash no nome da URL."
        )]
        [SwaggerResponse(200, "Foto removida com sucesso")]
        [SwaggerResponse(404, "Entrada não encontrada")]
        public async Task<IActionResult> DeletePhoto(Guid id, string fotoId)
        {
            var entry = await _service.GetByIdAsync(id);
            if (entry == null) return NotFound();

            entry.Photos = entry.Photos.Where(p => !p.Contains(fotoId)).ToList();
            await _service.UpdateAsync(id, entry, id);

            return Ok(new { success = true, message = "Foto removida com sucesso" });
        }

        [HttpGet("veiculo/{id}/fotos")]
        [SwaggerOperation(
            Summary = "Obtém fotos associadas a um veículo",
            Description = "Retorna as URLs das fotos associadas ao veículo."
        )]
        [SwaggerResponse(200, "Fotos do veículo", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Veículo não encontrado")]
        public async Task<IActionResult> GetPhotos(Guid id)
        {
            var photos = await _service.GetPhotosAsync(id);
            return Ok(photos);
        }

        private async Task<List<string>> UploadPhotosToSupabase(List<IFormFile> files)
        {
            return await _uploader.UploadAsync(files);
        }

    }
}
