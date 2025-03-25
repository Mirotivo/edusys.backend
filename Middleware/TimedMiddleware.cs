using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class TimedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _name;
    private readonly ILogger<TimedMiddleware> _logger;

    public TimedMiddleware(RequestDelegate next, string name, ILogger<TimedMiddleware> logger)
    {
        _next = next;
        _name = name;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();
        _logger.LogInformation($"Middleware '{_name}' took {sw.ElapsedMilliseconds} ms");
    }
}

