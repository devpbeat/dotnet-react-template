using Backend.Application.Interfaces;
using Hangfire;

namespace Backend.Application.Jobs;

public class TokenCleanupJob
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(
        ITokenService tokenService,
        ILogger<TokenCleanupJob> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredTokens()
    {
        _logger.LogInformation("Starting expired refresh tokens cleanup");

        try
        {
            await _tokenService.CleanupExpiredTokensAsync();
            _logger.LogInformation("Expired refresh tokens cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refresh tokens cleanup");
            throw;
        }
    }
}
