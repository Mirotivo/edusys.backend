using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Backend.Interfaces;
using Backend.Interfaces.Billing;
using Backend.Middleware;
using Backend.Services.BackgroundServices;
using Backend.Services.PaymentBackgroundServices;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
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

            // Configurations
            builder.Services.AddConfigurationMappings(builder.Configuration);
            // Add Serilog to the ASP.NET Core logging pipeline
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddSignalR();
            builder.Services.AddTransient<GlobalExceptionMiddleware>();
            builder.Services.AddTransient<GeolocationMiddleware>();
            builder.Services.AddSingleton<Dictionary<string, string>>(new Dictionary<string, string>());
            builder.Services.AddAvanciraDbContext(builder.Configuration, builder.Environment);
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
            builder.Services.AddTransients(Assembly.GetExecutingAssembly());
            builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddScoped<PayPalPaymentGateway>();
                builder.Services.AddScoped<StripePaymentGateway>();
            }
            builder.Services.AddScoped<INotificationChannel, SignalRNotificationChannel>();
            builder.Services.AddScoped<INotificationChannel, SmsNotificationChannel>();
            builder.Services.AddScoped<INotificationChannel, EmailNotificationChannel>();
            builder.Services.AddSingleton<IEmailService, SendEmailService>();
            builder.Services.AddSingleton<ISmsService, TwilioSmsService>();
            builder.Services.AddScoped<PaymentHourlyService>();
            builder.Services.AddHostedService<PaymentHourlyService>();
            builder.Services.AddScoped<PaymentDailyService>();
            builder.Services.AddHostedService<PaymentDailyService>();
            builder.Services.AddScoped<PaymentMonthlyService>();
            builder.Services.AddHostedService<PaymentMonthlyService>();
            builder.Services.AddHostedService<EventProcessorService>();
            builder.Services.AddEvents(builder.Configuration);

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
                            Console.WriteLine("Authentication failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Token validation succeeded
                            if (context != null && context.Principal != null && context.Principal.Identity != null)
                                Console.WriteLine("Token validated: " + context.Principal.Identity.Name);
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
                            Console.WriteLine("Message received: " + context.Token);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            // Authentication challenge event
                            Console.WriteLine("Authentication challenge: " + context.Error);
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            // Authorization failure event
                            Console.WriteLine("Authorization failed: " + context);
                            return Task.CompletedTask;
                        }
                    };
                });
                //.AddGoogle(options =>
                //{
                //    var googleOptions = builder.Configuration.GetSection("Avancira:ExternalServices:Google").Get<GoogleOptions>();
                //    options.ClientId = googleOptions?.ClientId ?? string.Empty;
                //    options.ClientSecret = googleOptions?.ClientSecret ?? string.Empty;
                //    options.CallbackPath = "/api/users/social-login";
                //})
                //.AddFacebook(options =>
                //{
                //    var facebookOptions = builder.Configuration.GetSection("Avancira:ExternalServices:Facebook").Get<FacebookOptions>();
                //    options.AppId = facebookOptions?.AppId ?? string.Empty;
                //    options.AppSecret = facebookOptions?.AppSecret ?? string.Empty;
                //    options.CallbackPath = "/api/users/social-login";
                //});

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
            app.InitializeDatabase(app.Environment);
            // Add Prometheus metrics
            app.UseHttpMetrics();  // Measures HTTP request duration and counts
            app.UseMetricServer(); // Exposes metrics at /metrics endpoint
            app.UseSerilogRequestLogging(); // This logs HTTP requests using Serilog

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFilesUploads();
            app.UseCookiePolicy();
            var anyOrigin = false;
            app.UseCors(options =>
            {
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
            app.UseRouting();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<GeolocationMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/notification");
            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to the dev page!");
            });
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
