using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Middleware
{
    public class MemberOnlyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Resolve SubscriptionService from the DI container
            var subscriptionService = context.HttpContext.RequestServices.GetRequiredService<ISubscriptionService>();

            if (!await subscriptionService.HasActiveSubscriptionAsync(userId))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
