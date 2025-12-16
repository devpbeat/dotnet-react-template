using Hangfire.Dashboard;

namespace Backend.Infrastructure.Configuration;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all access - we will only use this filter in Development environment
        return true;
    }
}
