using ApiCrmAlive.DTOs.Integrations;
using ApiCrmAlive.Services.Leads;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiCrmAlive.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsappController(ILeadService leadService, ILogger<WhatsappController> logger) : ControllerBase
    {
        [HttpPost("receive-message")]
        public async Task<IActionResult> ReceiveWhatsappMessage([FromBody] JsonElement body)
        {
            try
            {
                logger.LogInformation("Webhook recebido da Evolution API: {@Body}", body);

                // Verifica se é o evento correto
                if (!body.TryGetProperty("event", out var eventProp) ||
                    eventProp.GetString() != "MESSAGES_UPSERT")
                {
                    logger.LogWarning("Evento não é MESSAGES_UPSERT: {Event}", eventProp.GetString());
                    return Ok("Evento ignorado.");
                }

                // Acessa data.messages (array de mensagens)
                if (!body.TryGetProperty("data", out var data) ||
                    !data.TryGetProperty("messages", out var messagesProp) ||
                    messagesProp.ValueKind != JsonValueKind.Array)
                {
                    logger.LogWarning("Payload inválido: sem 'data.messages'.");
                    return BadRequest("Payload inválido.");
                }

                // Processa cada mensagem (geralmente uma, mas pode ser múltipla)
                foreach (var messageObj in messagesProp.EnumerateArray())
                {
                    // Ignora mensagens enviadas por você (fromMe: true)
                    if (messageObj.TryGetProperty("key", out var key) &&
                        key.TryGetProperty("fromMe", out var fromMe) &&
                        fromMe.GetBoolean())
                    {
                        logger.LogInformation("Ignorando mensagem enviada por mim.");
                        continue;
                    }

                    // Extrai telefone (remove @s.whatsapp.net)
                    string phone = key.GetProperty("remoteJid").GetString()!;
                    phone = phone.Split('@')[0]; // Ex: 5511999999999

                    // Nome do contato
                    string name = messageObj.TryGetProperty("pushName", out var pushName)
                        ? pushName.GetString() ?? "Desconhecido"
                        : "Desconhecido";

                    // Extrai texto da mensagem (fallback para tipos comuns)
                    string message = ExtractMessageText(messageObj);

                    var whatsappDto = new WhatsappMessageDto
                    {
                        ContactPhone = phone,
                        ContactName = name,
                        Message = message
                    };

                    logger.LogInformation("Processando lead: {Phone} - {Name} - {Message}", phone, name, message);

                    // Verifica lead existente
                    var existingLead = await leadService.GetByPhoneAsync(whatsappDto.ContactPhone);
                    if (existingLead != null)
                    {
                        logger.LogInformation("Lead já existente para {Phone}", phone);
                        return Ok("Lead já existente.");
                    }

                    // Cria novo lead
                    await leadService.CreateFromWhatsappAsync(whatsappDto);
                    logger.LogInformation("Lead criado com sucesso para {Phone}", phone);
                }

                return Ok("Mensagens processadas com sucesso.");
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(jsonEx, "Erro ao parsear JSON do webhook.");
                return BadRequest($"Erro no JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar webhook.");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // Método auxiliar para extrair texto de diferentes tipos de mensagem
        private static string ExtractMessageText(JsonElement messageObj)
        {
            if (!messageObj.TryGetProperty("message", out var msgContent))
                return string.Empty;

            // Texto simples
            if (msgContent.TryGetProperty("conversation", out var conv))
                return conv.GetString() ?? string.Empty;

            // Texto estendido (com formatação, links etc.)
            if (msgContent.TryGetProperty("extendedTextMessage", out var ext) &&
                ext.TryGetProperty("text", out var text))
                return text.GetString() ?? string.Empty;

            // Para outros tipos (imagem, áudio etc.), retorne um placeholder ou ignore
            // Ex: if (msgContent.TryGetProperty("imageMessage", out _)) return "[Imagem recebida]";
            return "[Tipo de mensagem não suportado]";
        }
    }
}