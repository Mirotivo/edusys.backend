using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

public class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Enable buffering for the request body
        context.Request.EnableBuffering();

        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogException(context, ex);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (Exception ex)
        {
            LogException(context, ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }
    }

    private void LogException(HttpContext context, Exception exception)
    {
        // Extract route information
        var routeData = context.GetRouteData();
        var controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
        var action = routeData.Values["action"]?.ToString() ?? "UnknownAction";

        // Log request details
        var requestDetails = new
        {
            Url = context.Request.GetDisplayUrl(),
            QueryString = context.Request.QueryString.ToString(),
            Method = context.Request.Method,
            Body = GetRequestBody(context), // Get the request body
            Controller = controller,
            Action = action
        };

        // Log the exception with request details
        _logger.LogError(exception, "An error occurred in {Controller}/{Action}. Request details: {RequestDetails}", controller, action, requestDetails);
    }

    private string GetRequestBody(HttpContext context)
    {
        try
        {
            if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin); // Rewind the stream
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = reader.ReadToEnd();
                context.Request.Body.Seek(0, SeekOrigin.Begin); // Reset for further use

                // Mask sensitive data if the content type is JSON
                if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(body);
                        var sanitizedJson = MaskSensitiveData(json);
                        return sanitizedJson;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse and sanitize JSON body.");
                    }
                }

                return body;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reading the request body.");
        }
        return "Unable to read request body.";
    }

    private string MaskSensitiveData(System.Text.Json.JsonDocument json)
    {
        var root = json.RootElement;
        var dictionary = new Dictionary<string, object>();

        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                dictionary[property.Name] = "*****";
            }
            else
            {
                dictionary[property.Name] = property.Value.GetRawText();
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(dictionary);
    }

}
