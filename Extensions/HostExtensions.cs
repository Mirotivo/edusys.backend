using System.Reflection;
using Backend.Interfaces.Events;
using Microsoft.AspNetCore.Identity;

public static class HostExtensions
{
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
