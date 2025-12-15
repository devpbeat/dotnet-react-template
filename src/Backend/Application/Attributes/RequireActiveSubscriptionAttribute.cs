using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Application.Attributes;

public class RequireActiveSubscriptionAttribute : TypeFilterAttribute
{
    public RequireActiveSubscriptionAttribute() : base(typeof(RequireActiveSubscriptionFilter))
    {
    }
}

public class RequireActiveSubscriptionFilter : IAsyncAuthorizationFilter
{
    // In a real app, inject DbContext or a Service to check subscription
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Logic to check if the user has an active subscription
        // 1. Get User ID from Claims
        // 2. Check DB for active subscription
        
        // For now, we pass.
        // context.Result = new ForbidResult(); // Uncomment to block
        
        return Task.CompletedTask;
    }
}
