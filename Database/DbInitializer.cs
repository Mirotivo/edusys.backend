using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static void Initialize(AvanciraDbContext context, UserManager<User> userManager)
    {
        context.Database.EnsureDeleted();

        context.Database.EnsureCreated();

        RoleSeeder.Seed(context, userManager);
        CountrySeeder.Seed(context, userManager);
        UserSeeder.Seed(context, userManager);
        LessonCategorySeeder.Seed(context, userManager);
        ListingSeeder.Seed(context, userManager);
        ListingLessonCategorySeeder.Seed(context, userManager);

        PromoCodeSeeder.Seed(context);
        // LessonSeeder.Seed(context);
        // ReviewSeeder.Seed(context);
        // ChatSeeder.Seed(context);
        // MessageSeeder.Seed(context);
    }
}

