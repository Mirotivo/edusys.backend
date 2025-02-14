using System.Reflection;
using Backend.Interfaces.Events;

public static class ConfigurationExtensions
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


    public static void AddEvents(this IServiceCollection Services, IConfiguration Configuration)
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
}
