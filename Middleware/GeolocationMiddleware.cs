using Backend.Interfaces;
using Backend.Services;
using Microsoft.AspNetCore.Identity;

namespace Backend.Middleware
{
    public class GeolocationMiddleware : IMiddleware
    {
        private readonly IGeolocationService _geolocationService;

        public GeolocationMiddleware(IGeolocationService geolocationService)
        {
            _geolocationService = geolocationService;
        }

        // InvokeAsync method to process the request and call the next middleware
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var country = await _geolocationService.GetCountryFromIpAsync(ipAddress);

            // Optionally, modify response headers or perform other actions
            context.Response.Headers["Country"] = country;

            // Call the next middleware in the pipeline
            await next(context);
        }
    }
}
