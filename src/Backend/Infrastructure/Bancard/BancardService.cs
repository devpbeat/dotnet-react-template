using Backend.Application.Interfaces;

namespace Backend.Infrastructure.Bancard;

public class BancardService : IBancardService
{
    public Task<string> CreateCatastroRequestAsync(Guid userId, string plan)
    {
        // Call vPOS to generate process_id
        // In a real implementation, this would make an HTTP POST to Bancard's API
        return Task.FromResult("dummy_process_id_" + Guid.NewGuid());
    }

    public Task<bool> ProcessWebhookAsync(string payload)
    {
        // Verify signature, parse payload, etc.
        return Task.FromResult(true);
    }
}
