using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

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
}
