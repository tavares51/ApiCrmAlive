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
    public async Task<IActionResult> ReceiveWhatsappMessage([FromBody] EvolutionWebhookPayload evoPayload)
    {
        if (evoPayload == null || evoPayload.Key?.RemoteJid == null)
            return BadRequest("Payload inválido");

        string phone = evoPayload.Key.RemoteJid; // ex: "5515998115496@s.whatsapp.net"
                                                 // Opcional: remover sufixo “@s.whatsapp.net”
        phone = phone.Split('@')[0];

        string name = evoPayload.PushName!;
        string message = evoPayload.Message?.Conversation!;

        // mapping para seu DTO
        var whatsappDto = new WhatsappMessageDto
        {
            ContactPhone = phone,
            ContactName = name,
            Message = message
        };

        // resto do seu fluxo
        var existingLead = await _leadService.GetByPhoneAsync(whatsappDto.ContactPhone);
        if (existingLead != null)
            return Ok("Lead já existente.");

        await _leadService.CreateFromWhatsappAsync(whatsappDto);
        return Ok("Lead criada com sucesso.");
    }
}