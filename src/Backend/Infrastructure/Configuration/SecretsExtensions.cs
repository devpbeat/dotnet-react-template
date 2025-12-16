namespace Backend.Infrastructure.Configuration;

public static class SecretsExtensions
{
    public static async Task<WebApplicationBuilder> ConfigureSecretsAsync(this WebApplicationBuilder builder)
    {
        var useInfisical = Environment.GetEnvironmentVariable("USE_INFISICAL")?.ToLower() == "true";

        if (useInfisical)
        {
            builder.Services.AddSingleton<ISecretsManager, InfisicalSecretsManager>();
        }
        else
        {
            builder.Services.AddSingleton<ISecretsManager, EnvironmentSecretsManager>();
        }

        return builder;
    }

    public static string GetSecretOrDefault(this IConfiguration configuration, string key, string defaultValue = "")
    {
        // Try environment variable first
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
        {
            return envValue;
        }

        // Fall back to appsettings.json
        var configValue = configuration[key];
        return configValue ?? defaultValue;
    }

    public static string GetRequiredSecret(this IConfiguration configuration, string key)
    {
        var value = configuration.GetSecretOrDefault(key);
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Required secret '{key}' is not configured");
        }
        return value;
    }
}
