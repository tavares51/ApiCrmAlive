using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ApiCrmAlive.Services.Integrations;

public class EvolutionWhatsappService(HttpClient http, ILogger<EvolutionWhatsappService> logger, IConfiguration config) : IEvolutionWhatsappService
{
    private readonly HttpClient _http = http;
    private readonly ILogger<EvolutionWhatsappService> _logger = logger;
    private readonly string _messagesEndpoint = config["Evolution:MessagesEndpoint"] ?? "message/sendText/crm";
    private readonly string _apiKey = config["Evolution:ApiKey"] ?? throw new InvalidOperationException("API Key não configurada.");

    public async Task SendTextMessageAsync(string toPhone, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhone))
        {
            _logger.LogWarning("Telefone destinatário vazio. Mensagem não enviada.");
            return;
        }

        // Normaliza o telefone para conter apenas dígitos (E.164 sem '+')
        var to = NormalizePhone(toPhone);
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("Telefone destinatário inválido após normalização. Mensagem não enviada. Origem: {Raw}", toPhone);
            return;
        }

        var payload = new
        {
            number = to,
            text = text
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _messagesEndpoint)
            {
                Content = JsonContent.Create(payload) // Define o Content-Type automaticamente como application/json
            };
            request.Headers.Add("apikey", _apiKey);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Falha ao enviar mensagem via Evolution: {Status} {Body}", response.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("Mensagem enviada via Evolution para {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem via Evolution API");
            throw;
        }
    }

    private static string NormalizePhone(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits;
    }
}