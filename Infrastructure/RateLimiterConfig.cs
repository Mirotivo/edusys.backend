using System;
using System.Threading.RateLimiting;
using Backend.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Infrastructure
{
    public static class RateLimiterConfig
    {
        public static void ConfigureRateLimiting(RateLimiterOptions options)
        {
            // Apply Global Rate Limiting (Default Policy)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: IpHelper.GetClientIp(context),
                    factory: _ => GetDefaultRateLimit()));

            // Stricter Rate Limit for Login API
            options.AddPolicy("login-policy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: IpHelper.GetClientIp(context),
                    factory: _ => GetLoginRateLimit()));


            // Custom Response for Rate Limit Exceeded
            options.OnRejected = static async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    "{\"error\": \"Too many requests. Please try again later.\"}",
                    cancellationToken);
            };
        }

        /// <summary>
        /// Default Rate Limiting (Applies to All APIs Unless Overridden)
        /// </summary>
        private static SlidingWindowRateLimiterOptions GetDefaultRateLimit() => new()
        {
            PermitLimit = 10,                 // Allow 10 requests
            Window = TimeSpan.FromSeconds(30), // Sliding window of 30 seconds
            SegmentsPerWindow = 3,            // 3 segments (each 10 sec)
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5                   // Allow queuing of 5 extra requests
        };

        /// <summary>
        /// Stricter Rate Limit for Login API
        /// </summary>
        private static FixedWindowRateLimiterOptions GetLoginRateLimit() => new()
        {
            PermitLimit = 3,                   // Allow 3 login attempts
            Window = TimeSpan.FromMinutes(1)   // 1-minute cooldown
        };
    }
}

