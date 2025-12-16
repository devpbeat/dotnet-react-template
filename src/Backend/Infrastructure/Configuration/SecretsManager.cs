using Infisical.Sdk;

namespace Backend.Infrastructure.Configuration;

public interface ISecretsManager
{
    Task<string?> GetSecretAsync(string key);
    Task<Dictionary<string, string>> GetAllSecretsAsync();
}

// For production: Use this with Infisical SDK (currently disabled due to v3.x API changes)
// TODO: Re-enable when Infisical SDK v3.x is properly integrated
public class InfisicalSecretsManager : ISecretsManager
{
    private readonly ILogger<InfisicalSecretsManager> _logger;

    public InfisicalSecretsManager(IConfiguration configuration, ILogger<InfisicalSecretsManager> logger)
    {
        _logger = logger;
        _logger.LogInformation("Using environment variables for secrets (Infisical SDK v3.x requires API update)");
    }

    public Task<string?> GetSecretAsync(string key)
    {
        // Temporarily using environment variables only until Infisical SDK v3.x is integrated
        return Task.FromResult(Environment.GetEnvironmentVariable(key));
    }

    public Task<Dictionary<string, string>> GetAllSecretsAsync()
    {
        // Temporarily using environment variables only until Infisical SDK v3.x is integrated
        _logger.LogInformation("Using environment variables for secrets");
        return Task.FromResult(new Dictionary<string, string>());
    }
}

// Alternative: Environment Variables Only (no external service)
public class EnvironmentSecretsManager : ISecretsManager
{
    private readonly ILogger<EnvironmentSecretsManager> _logger;

    public EnvironmentSecretsManager(ILogger<EnvironmentSecretsManager> logger)
    {
        _logger = logger;
        _logger.LogInformation("Using environment variables for secrets management");
    }

    public Task<string?> GetSecretAsync(string key)
    {
        return Task.FromResult(Environment.GetEnvironmentVariable(key));
    }

    public Task<Dictionary<string, string>> GetAllSecretsAsync()
    {
        // Return empty - environment variables are accessed directly
        return Task.FromResult(new Dictionary<string, string>());
    }
}
