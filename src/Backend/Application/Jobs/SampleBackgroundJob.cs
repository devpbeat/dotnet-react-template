using Hangfire;

namespace Backend.Application.Jobs;

// Example Hangfire Background Job
public class SampleBackgroundJob
{
    private readonly ILogger<SampleBackgroundJob> _logger;

    public SampleBackgroundJob(ILogger<SampleBackgroundJob> logger)
    {
        _logger = logger;
    }

    // Fire-and-forget job
    public void ProcessSubscriptionRenewal(Guid subscriptionId)
    {
        _logger.LogInformation(
            "Processing subscription renewal for {SubscriptionId}",
            subscriptionId);

        // TODO: Add actual subscription renewal logic
        // 1. Check subscription status
        // 2. Charge payment method
        // 3. Update subscription dates
        // 4. Send confirmation email

        _logger.LogInformation(
            "Completed subscription renewal for {SubscriptionId}",
            subscriptionId);
    }

    // Delayed job
    [AutomaticRetry(Attempts = 3)]
    public async Task SendWelcomeEmail(Guid userId, string email)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        // Simulate email sending
        await Task.Delay(1000);

        _logger.LogInformation("Welcome email sent to {Email}", email);
    }

    // Recurring job
    public async Task CleanupExpiredSubscriptions()
    {
        _logger.LogInformation("Starting cleanup of expired subscriptions");

        // TODO: Add cleanup logic
        // 1. Find all expired subscriptions
        // 2. Update their status
        // 3. Notify users
        // 4. Archive old data

        await Task.CompletedTask;

        _logger.LogInformation("Completed cleanup of expired subscriptions");
    }
}

// Extension method to register recurring jobs
public static class HangfireJobsExtensions
{
    public static void RegisterRecurringJobs(this IApplicationBuilder app)
    {
        // Register recurring jobs here
        // Example: Run cleanup every day at 2 AM
        RecurringJob.AddOrUpdate<SampleBackgroundJob>(
            "cleanup-expired-subscriptions",
            job => job.CleanupExpiredSubscriptions(),
            Cron.Daily(2));
    }
}
