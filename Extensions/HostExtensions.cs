using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

