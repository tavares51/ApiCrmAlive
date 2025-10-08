using ApiCrmAlive.DTOs.Integrations;
using ApiCrmAlive.Services.LeadInteraction;
using ApiCrmAlive.Services.Leads;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(ILeadService leadService, ILeadInteractionService leadInteractionService) : ControllerBase
{
    private readonly ILeadService _leadService = leadService;
    private readonly ILeadInteractionService _leadInteractionService = leadInteractionService;

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveWhatsappMessage([FromBody] JsonElement payload)
    {
        try
        {
            var data = payload.GetProperty("body").GetProperty("data");

            string phone = data.GetProperty("key").GetProperty("remoteJid").GetString()!;
            phone = phone.Split('@')[0];

            string name = data.GetProperty("pushName").GetString()!;
            string message = data.GetProperty("message").GetProperty("conversation").GetString()!;

            var whatsappDto = new WhatsappMessageDto
            {
                ContactPhone = phone,
                ContactName = name,
                Message = message
            };

            var existingLead = await _leadService.GetByPhoneAsync(whatsappDto.ContactPhone);
            if (existingLead != null)
                return Ok("Lead já existente.");

            await _leadService.CreateFromWhatsappAsync(whatsappDto);
            return Ok("Lead criada com sucesso.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao processar webhook: {ex.Message}");
        }
    }

}