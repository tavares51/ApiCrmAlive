using System.Text.Json;
using ApiCrmAlive.DTOs.Marketplaces.MercadoLivre;
using ApiCrmAlive.Services.Marketplaces.MercadoLivre;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MercadoLivreAuthController(IMercadoLivreAuthService service) : ControllerBase
{
    private readonly IMercadoLivreAuthService _service = service;

    [HttpPost("authorize")]
    [SwaggerOperation(Summary = "Autoriza via código do Mercado Livre (OAuth2)")]
    [SwaggerResponse(200, "Token gerado com sucesso", typeof(MlTokenDto))]
    public async Task<ActionResult<MlTokenDto>> Authorize([FromBody] string code, CancellationToken ct)
        => Ok(await _service.AuthorizeAsync(code, ct));

    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Atualiza o access_token usando refresh_token")]
    [SwaggerResponse(200, "Token atualizado com sucesso", typeof(MlTokenDto))]
    public async Task<ActionResult<MlTokenDto>> Refresh([FromBody] RefreshToken refreshToken, CancellationToken ct)
        => Ok(await _service.RefreshAsync(refreshToken, ct));

    [HttpGet("check_token")]
    [SwaggerOperation(Summary = "Valida e retorna o access_token atual")]
    [SwaggerResponse(200, "Token válido ou renovado", typeof(MlTokenDto))]
    public async Task<ActionResult<MlTokenDto?>> CheckToken(CancellationToken ct)
        => Ok(await _service.CheckTokenAsync(ct));

    [HttpPost("insert_vehicle")]
    [SwaggerOperation(Summary = "Insere Veiculo")]
    [SwaggerResponse(200, "Veículo Inserido", typeof(JsonElement))]
    public async Task<ActionResult<JsonElement?>> InsertVehicle([FromBody] MlPublishDto dto, CancellationToken ct)
        => Ok(await _service.InsertVehicleAsync(dto, ct));
}