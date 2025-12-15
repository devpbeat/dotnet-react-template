using Hangfire.Dashboard;

namespace Backend.Infrastructure.Hangfire;

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
