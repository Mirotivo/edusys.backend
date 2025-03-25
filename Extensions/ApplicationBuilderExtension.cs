using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }

    public static IApplicationBuilder UsePrometheus(this IApplicationBuilder app)
    {
        app.UseHttpMetrics();  // Prometheus metrics: Measures HTTP request duration and counts
        app.UseMetricServer(); // Prometheus metrics: Exposes metrics at /metrics endpoint
        return app;
    }

    public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder app)
    {
        app.UseCors(options =>
        {
            var anyOrigin = false;
            if (anyOrigin)
            {
                options.AllowAnyOrigin();
            }
            else
            {
                options.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://localhost:8000",
                    "https://localhost:8000",
                    "http://localhost:8080",
                    "https://localhost:8080",
                    "http://localhost:9000/",
                    "https://localhost:9000/",
                    "http://97.74.95.95:80",
                    "http://97.74.95.95:8000",
                    "https://avancira.com",
                    "https://www.avancira.com"
                );
                options.AllowCredentials();
            }
            options.AllowAnyMethod();
            options.AllowAnyHeader();
        });
        return app;
    }


    public static IApplicationBuilder UseStaticFilesUploads(this IApplicationBuilder app)
    {
        // Define the uploads directory
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Ensure the directory exists
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        // Configure static file serving for uploads
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/api/uploads",
            OnPrepareResponse = ctx =>
            {
                Console.WriteLine($"Serving file: {ctx.File.PhysicalPath}");
            }
        });

        return app;
    }

    public static IApplicationBuilder UseTimed(this IApplicationBuilder app, string name, Action<IApplicationBuilder> configure)
    {
        var builder = app.New();
        configure(builder);
        var branch = builder.Build();

        return app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetService<ILogger<TimedMiddleware>>();
            var timed = new TimedMiddleware(branch, name, logger);
            await timed.Invoke(context);
            await next();
        });
    }
}

