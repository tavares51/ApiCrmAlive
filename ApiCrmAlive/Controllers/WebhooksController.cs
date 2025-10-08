using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiCrmAlive.DTOs.Integrations;
using ApiCrmAlive.Services.Leads;
using ApiCrmAlive.Services.LeadInteraction;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ILeadInteractionService _leadInteractionService;

    public WebhooksController(ILeadService leadService, ILeadInteractionService leadInteractionService)
    {
        _leadService = leadService;
        _leadInteractionService = leadInteractionService;
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveWhatsappMessage()
    {
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        // Log bruto para inspeção
        Console.WriteLine("RAW Webhook Body: " + rawBody);

        JArray array;
        try
        {
            array = JArray.Parse(rawBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao parsear como array: " + ex);
            return BadRequest("JSON não é um array: " + ex.Message);
        }

        if (array.Count == 0)
        {
            return BadRequest("Array de payload vazio");
        }

        // Pega o primeiro elemento do array
        JObject wrapper = (JObject)array[0];

        // Dentro dele, procurar "body"
        if (!wrapper.TryGetValue("body", out JToken bodyToken))
        {
            return BadRequest("Não achei propriedade 'body' no wrapper");
        }

        JObject body = (JObject)bodyToken;

        // Dentro de body, procurar "data"
        if (!body.TryGetValue("data", out JToken dataToken))
        {
            return BadRequest("Não achei propriedade 'data' no body");
        }

        JObject data = (JObject)dataToken;

        // Agora extrair os campos desejados
        string remoteJid = data.SelectToken("key.remoteJid")?.ToString();
        if (remoteJid == null)
        {
            return BadRequest("remoteJid não encontrado");
        }
        string phone = remoteJid.Split('@')[0];

        string pushName = data.SelectToken("pushName")?.ToString();
        string message = data.SelectToken("message.conversation")?.ToString();

        // Crie seu DTO
        var whatsappDto = new WhatsappMessageDto
        {
            ContactPhone = phone,
            ContactName = pushName,
            Message = message
        };

        try
        {
            var existing = await _leadService.GetByPhoneAsync(whatsappDto.ContactPhone);
            if (existing != null)
            {
                return Ok("Lead já existente.");
            }

            await _leadService.CreateFromWhatsappAsync(whatsappDto);
            return Ok("Lead criada com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro interno: " + ex);
            return StatusCode(500, "Erro interno: " + ex.Message);
        }
    }
}
