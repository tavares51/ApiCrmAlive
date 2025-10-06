namespace ApiCrmAlive.Services.Integrations;

public interface IEvolutionWhatsappService
{
    Task SendTextMessageAsync(string toPhone, string text, CancellationToken ct = default);
}