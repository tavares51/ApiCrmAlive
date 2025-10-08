namespace ApiCrmAlive.DTOs.Integrations;

public class EvolutionWebhookPayload
{
    public string? NumberId { get; set; }
    public WebhookKey? Key { get; set; }
    public string? PushName { get; set; }
    public WebhookMessagePayload? Message { get; set; }
    public string? MessageType { get; set; }
}
