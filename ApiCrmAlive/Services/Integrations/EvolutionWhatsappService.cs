using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace ApiCrmAlive.Services.Integrations;

public class EvolutionWhatsappService : IEvolutionWhatsappService
{
    private readonly HttpClient _http;
    private readonly ILogger<EvolutionWhatsappService> _logger;

    public EvolutionWhatsappService(HttpClient http, ILogger<EvolutionWhatsappService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task SendTextMessageAsync(string toPhone, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhone))
        {
            _logger.LogWarning("Telefone destinatário vazio. Mensagem não enviada.");
            return;
        }

        var payload = new
        {
            to = toPhone,
            type = "text",
            text = new { body = text }
        };

        // endpoint "messages" é um exemplo; configure via BaseAddress ou variável EVOLUTION_MESSAGES_ENDPOINT
        var endpoint = "messages";

        try
        {
            var resp = await _http.PostAsJsonAsync(endpoint, payload, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Falha ao enviar mensagem via Evolution: {Status} {Body}", resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem via Evolution API");
            throw;
        }
    }
}