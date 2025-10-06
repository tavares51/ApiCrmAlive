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
        // 1. Extrai o número do contato
        var phone = message.ContactPhone;

        // 2. Verifica se já existe um lead com esse telefone
        var lead = await _leadService.GetByPhoneAsync(phone!);
        if (lead != null)
            return Ok(); // Já existe, não faz nada

        // 3. Cria novo lead com origem WhatsApp
        await _leadService.CreateFromWhatsappAsync(message);

        return Ok();
    }
}