using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Utilities;
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
        context.Request.EnableBuffering();

        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status401Unauthorized, "Unauthorized access.");
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "Invalid request parameters.");
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, "Operation failed.");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, int statusCode, string message)
    {
        string traceId = Guid.NewGuid().ToString();
        LogException(context, ex, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new ApiResponse<object>(
            success: false,
            message: $"{message} (Trace ID: {traceId})",
            data: null!,
            errors: new Dictionary<string, List<string>> { { "error", new List<string> { ex.Message } } },
            traceId: traceId
        );

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private void LogException(HttpContext context, Exception exception, string traceId)
    {
        var routeData = context.GetRouteData();
        var controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
        var action = routeData.Values["action"]?.ToString() ?? "UnknownAction";

        var requestDetails = new
        {
            TraceId = traceId,
            Url = context.Request.GetDisplayUrl(),
            QueryString = context.Request.QueryString.ToString(),
            Method = context.Request.Method,
            Body = GetRequestBody(context),
            Controller = controller,
            Action = action
        };

        _logger.LogError(exception, "Error in {Controller}/{Action}. Trace ID: {TraceId}. Request details: {@RequestDetails}",
            controller, action, traceId, requestDetails);
    }

    private string GetRequestBody(HttpContext context)
    {
        try
        {
            if (!context.Request.Body.CanSeek)
                return "Request body is not readable.";

            context.Request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = reader.ReadToEnd();
            context.Request.Body.Seek(0, SeekOrigin.Begin);

            return context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true
                ? MaskSensitiveData(body)
                : body;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while reading request body.");
            return "Error reading request body.";
        }
    }

    private string MaskSensitiveData(string jsonString)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonString);
            var dictionary = new Dictionary<string, object>();

            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {
                dictionary[property.Name] = property.Name.Equals("password", StringComparison.OrdinalIgnoreCase)
                    ? "*****"
                    : property.Value.GetRawText();
            }

            return JsonSerializer.Serialize(dictionary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse and mask JSON request body.");
            return "Invalid JSON format.";
        }
    }
}
