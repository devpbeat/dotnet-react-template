namespace Backend.Infrastructure.Configuration;

public static class SecretsExtensions
{
    public static async Task<WebApplicationBuilder> ConfigureSecretsAsync(this WebApplicationBuilder builder)
    {
        // Load .env file if it exists (for local development)
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, "../../.env");
        if (File.Exists(dotenv))
        {
            foreach (var line in File.ReadAllLines(dotenv))
            {
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        else 
        {
             // Try looking in the current directory (for when running from root)
             dotenv = Path.Combine(root, ".env");
             if (File.Exists(dotenv))
             {
                foreach (var line in File.ReadAllLines(dotenv))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    Environment.SetEnvironmentVariable(key, value);
                }
             }
        }

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
