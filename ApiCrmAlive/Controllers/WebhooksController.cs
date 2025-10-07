using Microsoft.AspNetCore.Mvc;
using ApiCrmAlive.Services.Leads;
using ApiCrmAlive.DTOs.Integrations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(ILeadService leadService) : ControllerBase
{
    private readonly ILeadService _leadService = leadService;

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveWhatsappMessage([FromBody] WhatsappMessageDto message)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.ContactPhone))
            return BadRequest("Mensagem inválida.");

        // Verifica se o lead já existe pelo telefone
        var existingLead = await _leadService.GetByPhoneAsync(message.ContactPhone);
        if (existingLead != null)
        {
            return Ok("Lead já existente. Nenhuma ação necessária.");
        }

        // Cria um novo lead com base na mensagem recebida
        await _leadService.CreateFromWhatsappAsync(message);

        return Ok("Lead criada com sucesso.");
    }
}