using Microsoft.AspNetCore.Mvc;
using ApiCrmAlive.Services.Leads;
using ApiCrmAlive.DTOs.Integrations;
using ApiCrmAlive.Services.Integrations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(ILeadService leadService, IEvolutionWhatsappService evolutionService) : ControllerBase
{
    private readonly ILeadService _leadService = leadService;
    private readonly IEvolutionWhatsappService _evolutionService = evolutionService;

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveWhatsappMessage([FromBody] WhatsappMessageDto message)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.ContactPhone))
            return BadRequest();

        var phone = message.ContactPhone!.Trim();

        // Verifica se já existe um lead com esse telefone
        var existing = await _leadService.GetByPhoneAsync(phone);
        if (existing != null)
            return Ok(); // Já existe, não faz nada

        // Cria novo lead com origem WhatsApp
        var created = await _leadService.CreateFromWhatsappAsync(message);

        // Envia saudação via Evolution API para leads novas
        var contactName = string.IsNullOrWhiteSpace(created.Name) ? "" : created.Name;
        var greeting = $"Olá {contactName}, obrigado por entrar em contato! Em breve retornaremos.";
        try
        {
            await _evolutionService.SendTextMessageAsync(phone, greeting);
        }
        catch
        {
            // não falhar o webhook caso envio de resposta falhe
        }

        return Ok(created);
    }
}