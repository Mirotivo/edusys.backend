using System;
using System.Reflection;
using System.Text;
using AutoMapper;
using Backend.Interfaces;
using Backend.Interfaces.Billing;
using Backend.Middleware;
using Backend.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Prometheus;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurations
        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("Skillseek:App"));
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Skillseek:Jwt"));
        builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Skillseek:Payments:Stripe"));
        builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("Skillseek:Payments:PayPal"));
        builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("Skillseek:Notifications:SendGrid"));
        builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection("Skillseek:Notifications:Twilio"));
        builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Skillseek:ExternalServices:Google"));
        builder.Services.Configure<FacebookOptions>(builder.Configuration.GetSection("Skillseek:ExternalServices:Facebook"));

        // Add services to the container.
        builder.Services.AddSignalR();
        builder.Services.AddTransient<GlobalExceptionMiddleware>();
        builder.Services.AddTransient<GeolocationMiddleware>();
        builder.Services.AddSingleton<Dictionary<string, string>>(new Dictionary<string, string>());
        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<skillseekDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
            //options.UseInMemoryDatabase("TestDatabase")
            );
        }
        else
        {
            var connString = builder.Configuration.GetConnectionString("Sqlite") ?? "";
            builder.Services.AddDbContext<skillseekDbContext>(option => option.UseSqlite(connString));

            // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            // builder.Services.AddDbContext<skillseekDbContext>(options =>
            // {
            //     // Ensure the database is created
            //     options.UseSqlServer(connectionString);
            //     options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //     options.EnableSensitiveDataLogging();
            // });
        }
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<skillseekDbContext>()
            .AddDefaultTokenProviders();
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
        builder.Services.AddSingleton<IEmailService, SendGridEmailService>();
        builder.Services.AddSingleton<ISmsService, TwilioSmsService>();
        builder.Services.AddScoped<TimerBackgroundService>();
        builder.Services.AddHostedService<TimerBackgroundService>();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtOptions = builder.Configuration.GetSection("Skillseek:Jwt").Get<JwtOptions>();
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.Key ?? string.Empty)),
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

            })
            .AddGoogle(options =>
            {
                var googleOptions = builder.Configuration.GetSection("Skillseek:ExternalServices:Google").Get<GoogleOptions>();
                options.ClientId = googleOptions?.ClientId ?? string.Empty;
                options.ClientSecret = googleOptions?.ClientSecret ?? string.Empty;
                options.CallbackPath = "/signin-google";
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
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<skillseekDbContext>();
                DbInitializer.Initialize(dbContext);
            }
        }

        // Add Prometheus metrics
        app.UseMetricServer(); // Exposes metrics at /metrics endpoint
        app.UseHttpMetrics();  // Measures HTTP request duration and counts

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
            RequestPath = "/uploads",
            OnPrepareResponse = ctx =>
            {
                Console.WriteLine($"Serving file: {ctx.File.PhysicalPath}");
            }
        });
        app.UseCookiePolicy();
        app.UseCors(options =>
        {
            options.WithOrigins(
                "http://localhost:4200",
                "https://localhost:8000",
                "https://localhost:9000/",
                "https://92.205.162.126:8000",
                "http://110.144.148.168",
                "http://110.144.148.168:80",
                "http://110.144.148.168:8000"
            );
            // options.AllowAnyOrigin();
            options.AllowAnyMethod();
            options.AllowAnyHeader();
            options.AllowCredentials();
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
}
