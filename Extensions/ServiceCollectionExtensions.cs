using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all classes ending with "Service" as transients, along with their corresponding interfaces.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="assembly">The assembly to scan for services.</param>
    public static void AddTransients(this IServiceCollection services, Assembly assembly)
    {
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
            services.AddDbContext<AvanciraDbContext>(option => option.UseNpgsql(connString));
        }
        else if (dbProvider == "Sqlite")
        {
            services.AddDbContext<AvanciraDbContext>(option => option.UseSqlite(connString));
        }
        else if (dbProvider == "SqlServer")
        {
            services.AddDbContext<AvanciraDbContext>(options =>
            {
                options.UseSqlServer(connString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.EnableSensitiveDataLogging();
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

    public static void InitializeDatabase(this IHost app, IWebHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<AvanciraDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                DbInitializer.Initialize(dbContext, userManager);
            }
        }
    }
}
