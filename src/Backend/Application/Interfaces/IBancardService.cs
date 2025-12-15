namespace Backend.Application.Interfaces;

public interface IBancardService
{
    Task<string> CreateCatastroRequestAsync(Guid userId, string plan);
    Task<bool> ProcessWebhookAsync(string payload);
}
