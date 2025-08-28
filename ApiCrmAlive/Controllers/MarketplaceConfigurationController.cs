using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Services.Marketplaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/marketplaces/{marketplaceId:guid}/configuration")]
public class MarketplaceConfigurationController(IMarketplaceConfigurationService service) : ControllerBase
{
    private readonly IMarketplaceConfigurationService _service = service;

    // 🔹 GET configuração
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obter configuração de um marketplace",
        Description = "Retorna os dados da configuração vinculada a um marketplace."
    )]
    [SwaggerResponse(200, "Configuração encontrada com sucesso", typeof(MarketplaceConfigurationDto))]
    [SwaggerResponse(404, "Configuração não encontrada")]
    public async Task<IActionResult> GetConfiguration(Guid marketplaceId, CancellationToken ct)
    {
        var config = await _service.GetByMarketplaceIdAsync(marketplaceId, ct);
        return config is not null ? Ok(config) : NotFound();
    }

    // 🔹 POST configuração
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar configuração de marketplace",
        Description = "Cria uma nova configuração para o marketplace informado."
    )]
    [SwaggerResponse(201, "Configuração criada com sucesso", typeof(MarketplaceConfigurationDto))]
    public async Task<IActionResult> CreateConfiguration(
        Guid marketplaceId,
        [FromBody] MarketplaceConfigurationCreateDto dto,
        CancellationToken ct)
    {
        var userId = Guid.NewGuid(); // 🔹 aqui você injeta o usuário autenticado
        var result = await _service.CreateAsync(marketplaceId, dto, userId, ct);
        return CreatedAtAction(nameof(GetConfiguration), new { marketplaceId }, result);
    }

    // 🔹 PUT configuração
    [HttpPut]
    [SwaggerOperation(
        Summary = "Atualizar configuração de marketplace",
        Description = "Atualiza os dados da configuração existente."
    )]
    [SwaggerResponse(200, "Configuração atualizada com sucesso", typeof(MarketplaceConfigurationDto))]
    [SwaggerResponse(404, "Configuração não encontrada")]
    public async Task<IActionResult> UpdateConfiguration(
        Guid marketplaceId,
        [FromBody] MarketplaceConfigurationUpdateDto dto,
        CancellationToken ct)
    {
        var userId = Guid.NewGuid(); // 🔹 injeta o usuário autenticado
        var result = await _service.UpdateAsync(marketplaceId, dto, userId, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    // 🔹 POST Testar Conexão
    [HttpPost("test-connection")]
    [SwaggerOperation(
        Summary = "Testar conexão com marketplace",
        Description = "Executa um teste de conexão com o marketplace configurado."
    )]
    [SwaggerResponse(200, "Conexão testada com sucesso", typeof(TestConnectionResultDto))]
    [SwaggerResponse(400, "Falha ao testar a conexão")]
    public async Task<IActionResult> TestConnection(Guid marketplaceId, CancellationToken ct)
    {
        var result = await _service.TestConnectionAsync(marketplaceId, ct);
        return Ok(result);
    }
}
