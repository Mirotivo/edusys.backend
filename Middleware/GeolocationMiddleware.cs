using System.Threading.Tasks;
using Backend.Utilities;
using Backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Backend.Middleware
{
    public class GeolocationMiddleware : IMiddleware
    {
        private readonly IGeolocationService _geolocationService;

        public GeolocationMiddleware(IGeolocationService geolocationService)
        {
            _geolocationService = geolocationService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // var gatewayIp = context.Connection.RemoteIpAddress?.ToString();
            var clientIp = IpHelper.GetClientIp(context);
            var path = context.Request.Path.Value?.ToLower();
            if (path == "/api/users/register" || path == "/api/users/social-login")
            {
                Log.Information($"Client IP: {clientIp}");
                // Store the country in HttpContext.Items so it can be accessed in the controller
                var country = await _geolocationService.GetCountryFromIpAsync(clientIp);
                context.Items["Country"] = country ?? "AU";
            }

            await next(context);
        }
    }
}

