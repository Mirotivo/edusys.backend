using Microsoft.AspNetCore.Http;

namespace Backend.Utilities
{
    public static class IpHelper
    {
        private static readonly string[] IpHeaders = { "X-Forwarded-For", "X-Real-IP" };

        public static string GetClientIp(HttpContext context)
        {
            foreach (var header in IpHeaders)
            {
                if (context.Request.Headers.TryGetValue(header, out var value)
                    && !string.IsNullOrWhiteSpace(value))
                {
                    return value.ToString().Split(',')[0].Trim();
                }
            }

            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        }
    }
}

