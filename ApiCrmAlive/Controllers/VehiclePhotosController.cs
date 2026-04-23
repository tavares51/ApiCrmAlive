using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace ApiCrmAlive.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclePhotosController(IVehicleService service, IFileUploader uploader) : ControllerBase
    {
        private readonly IVehicleService _service = service;
        private readonly IFileUploader _uploader = uploader;
        private Guid GetActorUserIdOrThrow() => User.GetUserIdOrThrow();

        [HttpPost("veiculo/{id}/fotos")]
        [HttpPost("vehicles/{id:guid}/photos")]
        [SwaggerOperation(
            Summary = "Adiciona fotos a uma entrada de veículo",
            Description = "Envia uma ou mais fotos (form-data) para adicionar à entrada existente."
        )]
        [SwaggerResponse(200, "Fotos adicionadas com sucesso")]
        [SwaggerResponse(400, "Nenhuma foto válida foi enviada")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPhotos(Guid id, [FromForm] List<IFormFile> photos, CancellationToken ct)
        {
            var vehicle = await _service.GetByIdAsync(id, ct);
            if (vehicle == null) return NotFound();

            if (photos == null || !photos.Any(p => p.Length > 0))
                return BadRequest("Nenhuma foto válida foi enviada.");

            var newObjectNames = await UploadPhotos(photos);
            if (newObjectNames.Count == 0)
                return BadRequest("Nenhuma foto foi adicionada com sucesso.");

            // Obtém os object names já armazenados (não as URLs resolvidas do DTO)
            var existingObjectNames = (await _service.GetPhotosAsync(id, ct))
                .Select(NormalizePhotoReference)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
            existingObjectNames.AddRange(newObjectNames.Select(NormalizePhotoReference));
            existingObjectNames = existingObjectNames
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Salva os nomes dos objetos (paths) no banco, não as URLs presigned
            var updated = await _service.PatchAsync(
                id,
                new DTOs.Vehicles.VehicleUpdateDto { Photos = existingObjectNames },
                GetActorUserIdOrThrow(),
                ct);

            // Gera URLs frescas apenas para a resposta
            var freshUrls = await _uploader.GetUrlsAsync(existingObjectNames);
            return Ok(new { id = updated.Id, photos = freshUrls });
        }

        [HttpDelete("veiculo/{id}/fotos/{fotoId}")]
        [HttpDelete("vehicles/{id:guid}/photos/{fotoId}")]
        [SwaggerOperation(
            Summary = "Remove uma foto de uma entrada de veículo",
            Description = "Remove uma foto específica de uma entrada com base em seu ID ou hash no nome da URL."
        )]
        [SwaggerResponse(200, "Foto removida com sucesso")]
        [SwaggerResponse(404, "Entrada não encontrada")]
        public async Task<IActionResult> DeletePhoto(Guid id, string fotoId, CancellationToken ct)
        {
            var current = (await _service.GetPhotosAsync(id, ct))
                .Select(NormalizePhotoReference)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var filtered = current
                .Where(p => !p.Contains(fotoId, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            await _service.PatchAsync(
                id,
                new DTOs.Vehicles.VehicleUpdateDto { Photos = filtered },
                GetActorUserIdOrThrow(),
                ct);

            return Ok(new { success = true, message = "Foto removida com sucesso" });
        }

        [HttpGet("veiculo/{id}/fotos")]
        [HttpGet("vehicles/{id:guid}/photos")]
        [SwaggerOperation(
            Summary = "Obtém fotos associadas a um veículo",
            Description = "Retorna as URLs das fotos associadas ao veículo."
        )]
        [SwaggerResponse(200, "Fotos do veículo", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Veículo não encontrado")]
        public async Task<IActionResult> GetPhotos(Guid id, CancellationToken ct)
        {
            var objectNames = (await _service.GetPhotosAsync(id, ct))
                .Select(NormalizePhotoReference)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
            var urls = await _uploader.GetUrlsAsync(objectNames);
            return Ok(urls);
        }

        [HttpPut("veiculo/{id}/fotos/order")]
        [HttpPut("vehicles/{id:guid}/photos/order")]
        [SwaggerOperation(
            Summary = "Reordena fotos do veículo",
            Description = "Aceita { \"photos\": [\"...\"] } ou um array [\"...\"] com a nova ordem.")]
        [SwaggerResponse(200, "Ordem atualizada com sucesso")]
        [SwaggerResponse(400, "Payload inválido")]
        public async Task<IActionResult> ReorderPhotos(Guid id, [FromBody] JsonElement body, CancellationToken ct)
        {
            var requested = ExtractPhotosFromBody(body)
                .Select(NormalizePhotoReference)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (requested.Count == 0)
                return BadRequest("Informe a nova ordem das fotos no body.");

            var current = (await _service.GetPhotosAsync(id, ct))
                .Select(NormalizePhotoReference)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Reordena somente fotos existentes e preserva as restantes no final.
            var currentSet = new HashSet<string>(current, StringComparer.OrdinalIgnoreCase);
            var ordered = requested
                .Where(currentSet.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            ordered.AddRange(current.Where(p => !ordered.Contains(p, StringComparer.OrdinalIgnoreCase)));

            var updated = await _service.PatchAsync(
                id,
                new DTOs.Vehicles.VehicleUpdateDto { Photos = ordered },
                GetActorUserIdOrThrow(),
                ct);

            return Ok(new { id = updated.Id, photos = updated.Photos });
        }

        private async Task<List<string>> UploadPhotos(List<IFormFile> files)
        {
            return await _uploader.UploadAsync(files);
        }

        private static List<string> ExtractPhotosFromBody(JsonElement body)
        {
            if (body.ValueKind == JsonValueKind.Array)
                return body.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString() ?? string.Empty)
                    .ToList();

            if (body.ValueKind == JsonValueKind.Object)
            {
                if (body.TryGetProperty("photos", out var photos) && photos.ValueKind == JsonValueKind.Array)
                {
                    return photos.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String)
                        .Select(x => x.GetString() ?? string.Empty)
                        .ToList();
                }

                if (body.TryGetProperty("order", out var order) && order.ValueKind == JsonValueKind.Array)
                {
                    return order.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String)
                        .Select(x => x.GetString() ?? string.Empty)
                        .ToList();
                }
            }

            return [];
        }

        private static string NormalizePhotoReference(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            static string clean(string v)
            {
                var s = v.Trim();
                var q = s.IndexOf('?');
                if (q >= 0) s = s[..q];
                var h = s.IndexOf('#');
                if (h >= 0) s = s[..h];
                return s.TrimStart('/');
            }

            var normalizedValue = NormalizeHttpUrl(value);
            if (!Uri.TryCreate(normalizedValue, UriKind.Absolute, out var uri))
                return clean(normalizedValue);

            var path = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
            var uploadsIndex = path.IndexOf("uploads/", StringComparison.OrdinalIgnoreCase);
            if (uploadsIndex >= 0)
                return clean(path[uploadsIndex..]);

            if (TryExtractObjectNameFromStoragePath(path, out var objectName))
                return clean(objectName);

            // URL externa/legada: preserva o valor original sem forçar caminho de objeto.
            return clean(normalizedValue);
        }

        private static bool TryExtractObjectNameFromStoragePath(string path, out string objectName)
        {
            objectName = string.Empty;

            // storage/v1/object/public/{bucket}/{object}
            var publicMarker = "storage/v1/object/public/";
            var publicIndex = path.IndexOf(publicMarker, StringComparison.OrdinalIgnoreCase);
            if (publicIndex >= 0)
            {
                var tail = path[(publicIndex + publicMarker.Length)..];
                var firstSlash = tail.IndexOf('/');
                if (firstSlash >= 0 && firstSlash + 1 < tail.Length)
                {
                    objectName = tail[(firstSlash + 1)..];
                    return true;
                }
            }

            // storage/v1/object/sign/{bucket}/{object}
            var signMarker = "storage/v1/object/sign/";
            var signIndex = path.IndexOf(signMarker, StringComparison.OrdinalIgnoreCase);
            if (signIndex >= 0)
            {
                var tail = path[(signIndex + signMarker.Length)..];
                var firstSlash = tail.IndexOf('/');
                if (firstSlash >= 0 && firstSlash + 1 < tail.Length)
                {
                    objectName = tail[(firstSlash + 1)..];
                    return true;
                }
            }

            // storage/v1/object/{bucket}/{object}
            var objectMarker = "storage/v1/object/";
            var objectIndex = path.IndexOf(objectMarker, StringComparison.OrdinalIgnoreCase);
            if (objectIndex >= 0)
            {
                var tail = path[(objectIndex + objectMarker.Length)..];
                var firstSlash = tail.IndexOf('/');
                if (firstSlash >= 0 && firstSlash + 1 < tail.Length)
                {
                    objectName = tail[(firstSlash + 1)..];
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeHttpUrl(string value)
        {
            var trimmed = value.Trim();
            if (trimmed.StartsWith("https:/", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + trimmed["https:/".Length..].TrimStart('/');
            }

            if (trimmed.StartsWith("http:/", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return "http://" + trimmed["http:/".Length..].TrimStart('/');
            }

            return trimmed;
        }

    }
}
