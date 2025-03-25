using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Backend.Interfaces.Billing;
using Backend.Interfaces.Events;
using Backend.Middleware;
using Backend.Services.PaymentBackgroundServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    public static void AddConfigurationMappings(this IServiceCollection Services, IConfiguration Configuration)
    {
        Services.Configure<AppOptions>(Configuration.GetSection("Avancira:App"));
        Services.Configure<JwtOptions>(Configuration.GetSection("Avancira:Jwt"));
        Services.Configure<StripeOptions>(Configuration.GetSection("Avancira:Payments:Stripe"));
        Services.Configure<PayPalOptions>(Configuration.GetSection("Avancira:Payments:PayPal"));
        Services.Configure<EmailOptions>(Configuration.GetSection("Avancira:Notifications:Email"));
        Services.Configure<GraphApiOptions>(Configuration.GetSection("Avancira:Notifications:GraphApi"));
        Services.Configure<SmtpOpctions>(Configuration.GetSection("Avancira:Notifications:Smtp"));
        Services.Configure<SendGridOptions>(Configuration.GetSection("Avancira:Notifications:SendGrid"));
        Services.Configure<TwilioOptions>(Configuration.GetSection("Avancira:Notifications:Twilio"));
        Services.Configure<JitsiOptions>(Configuration.GetSection("Avancira:Jitsi"));
        Services.Configure<GoogleOptions>(Configuration.GetSection("Avancira:ExternalServices:Google"));
        Services.Configure<FacebookOptions>(Configuration.GetSection("Avancira:ExternalServices:Facebook"));
    }

    public static void AddTransients(this IServiceCollection services, Assembly assembly)
    {
        services.AddTransient<GlobalExceptionMiddleware>();
        services.AddTransient<GeolocationMiddleware>();

        var serviceTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Service") && t.IsClass && !t.IsAbstract);

        foreach (var implementationType in serviceTypes)
        {
            var interfaceType = implementationType.GetInterface($"I{implementationType.Name}");
            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, implementationType);
            }
        }
    }

    public static void AddScopedServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddScoped<INotificationChannel, SignalRNotificationChannel>();
        services.AddScoped<INotificationChannel, SmsNotificationChannel>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();

        services.AddScoped<PaymentHourlyService>();
        services.AddScoped<PaymentDailyService>();
        services.AddScoped<PaymentMonthlyService>();

        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddScoped<PayPalPaymentGateway>();
            services.AddScoped<StripePaymentGateway>();
        }
    }

    public static void AddScopedEvents(this IServiceCollection Services)
    {
        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface) // Exclude interfaces and abstract classes
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(i => new { InterfaceType = i, ImplementationType = t }));

        foreach (var handler in handlerTypes)
        {
            Services.AddScoped(handler.InterfaceType, handler.ImplementationType);
        }
    }

    public static void AddAvanciraDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var dbProvider = configuration["Avancira:Database:Provider"] ?? "Sqlite";
        var connString = configuration.GetConnectionString(dbProvider) ?? "";
        var pattern = @"\{([^}]+)\}";
        connString = Regex.Replace(connString, pattern, match =>
        {
            var envVar = match.Groups[1].Value;
            var envValue = Environment.GetEnvironmentVariable(envVar);
            return !string.IsNullOrEmpty(envValue) ? envValue : match.Value;
        });

        if (environment.IsEnvironment("Testing"))
        {
            services.AddDbContext<AvanciraDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            );
        }
        else if (dbProvider == "PostgreSql")
        {
            services.AddDbContext<AvanciraDbContext>(options =>
            {
                options.UseNpgsql(connString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.EnableSensitiveDataLogging(false);
            });
        }
        else if (dbProvider == "Sqlite")
        {
            services.AddDbContext<AvanciraDbContext>(options => options.UseSqlite(connString));
        }
        else if (dbProvider == "SqlServer")
        {
            services.AddDbContext<AvanciraDbContext>(options =>
            {
                options.UseSqlServer(connString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.EnableSensitiveDataLogging(false);
            });
        }
        else
        {
            services.AddDbContext<AvanciraDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            );
        }

        // Configure Identity
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AvanciraDbContext>()
            .AddDefaultTokenProviders();
    }
}

