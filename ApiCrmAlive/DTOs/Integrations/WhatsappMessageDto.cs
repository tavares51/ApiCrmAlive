namespace ApiCrmAlive.DTOs.Integrations;

public class WhatsappMessageDto
{
    public string? ContactPhone { get; set; }
    public string? ContactName { get; set; }
    public string? Message { get; set; }
    // Adicione outros campos conforme o payload da Evolution API
}