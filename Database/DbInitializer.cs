using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Initialize(AvanciraDbContext context, UserManager<User> userManager)
    {
        context.Database.EnsureDeleted();

        context.Database.EnsureCreated();

        RoleSeeder.Seed(context, userManager);
        UserSeeder.Seed(context, userManager);
        LessonCategorySeeder.Seed(context, userManager);
        ListingSeeder.Seed(context, userManager);
        // LessonSeeder.Seed(context);
        // ReviewSeeder.Seed(context);
        // ChatSeeder.Seed(context);
        // MessageSeeder.Seed(context);
    }
}
