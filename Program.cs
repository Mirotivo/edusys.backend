using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Backend.Infrastructure;
using Backend.Middleware;
using Backend.Services.BackgroundServices;
using Backend.Services.PaymentBackgroundServices;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;


public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog using appsettings.json
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build())
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add Serilog to the ASP.NET Core logging pipeline
            builder.Host.UseSerilog();

            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Configurations
            builder.Services.AddConfigurationMappings(builder.Configuration);

            // Add services to the container.
            builder.Services.AddSingleton<Dictionary<string, string>>([]);
            builder.Services.AddSingleton<IEmailService, SendEmailService>();
            builder.Services.AddSingleton<ISmsService, TwilioSmsService>();
            builder.Services.AddHostedService<PaymentHourlyService>();
            builder.Services.AddHostedService<PaymentDailyService>();
            builder.Services.AddHostedService<PaymentMonthlyService>();
            builder.Services.AddHostedService<EventProcessorService>();
            builder.Services.AddScopedServices(builder.Environment);
            builder.Services.AddScopedEvents();
            builder.Services.AddTransients(Assembly.GetExecutingAssembly());

            builder.Services.AddAvanciraDbContext(builder.Configuration, builder.Environment);
            builder.Services.AddSignalR();
            builder.Services.AddRateLimiter(RateLimiterConfig.ConfigureRateLimiting);
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtOptions = builder.Configuration.GetSection("Avancira:Jwt").Get<JwtOptions>();
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions?.Key ?? string.Empty)),
                        ValidIssuer = jwtOptions?.Issuer ?? string.Empty,
                        ValidAudience = jwtOptions?.Audience ?? string.Empty
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Log.Debug("Authentication failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Token validation succeeded
                            if (context != null && context.Principal != null && context.Principal.Identity != null)
                                Log.Debug("Token validated: " + context.Principal.Identity.Name);
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notification"))
                            {
                                context.Token = accessToken;
                            }

                            // Invoked when a WebSocket or Long Polling request is received.
                            Log.Debug("Message received: " + context.Token);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            // Authentication challenge event
                            Log.Debug("Authentication challenge: " + context.Error);
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            // Authorization failure event
                            Log.Debug("Authorization failed: " + context);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

                        var response = new
                        {
                            Message = "Validation failed",
                            Errors = errors
                        };

                        return new BadRequestObjectResult(response);
                    });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();


            // Middlewares: Configure the HTTP request pipeline.
            app.UseSwagger(app.Environment);
            app.UsePrometheus();
            app.UseSerilogRequestLogging(); // This logs HTTP requests using Serilog
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFilesUploads();
            app.UseCookiePolicy();
            app.UseCorsMiddleware();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<GeolocationMiddleware>();
            app.UseRateLimiter();


            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notification");
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to the dev page!");
                });
            });

            app.InitializeDatabase(app.Environment);
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Application failed to start.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

