using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Attributes
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

