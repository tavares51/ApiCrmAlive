using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclePhotosController(IVehicleService service, IFileUploader uploader) : ControllerBase
    {
        private readonly IVehicleService _service = service;
        private readonly IFileUploader _uploader = uploader;

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

            var newObjectNames = await UploadPhotos(photos);
            if (newObjectNames.Count == 0)
                return BadRequest("Nenhuma foto foi adicionada com sucesso.");

            // Obtém os object names já armazenados (não as URLs resolvidas do DTO)
            var existingObjectNames = await _service.GetPhotosAsync(id);
            existingObjectNames.AddRange(newObjectNames);

            // Salva os nomes dos objetos (paths) no banco, não as URLs presigned
            vehicle.Photos = existingObjectNames;
            await _service.UpdateAsync(id, vehicle, Guid.NewGuid());

            // Gera URLs frescas apenas para a resposta
            var freshUrls = await _uploader.GetUrlsAsync(existingObjectNames);
            return Ok(new { id = vehicle.Id, photos = freshUrls });
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
            var objectNames = await _service.GetPhotosAsync(id);
            var urls = await _uploader.GetUrlsAsync(objectNames);
            return Ok(urls);
        }

        private async Task<List<string>> UploadPhotos(List<IFormFile> files)
        {
            return await _uploader.UploadAsync(files);
        }

    }
}
