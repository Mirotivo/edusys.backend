public static class DbInitializer
{
    public static void Initialize(skillseekDbContext context)
    {
        context.Database.EnsureCreated();

        RoleSeeder.Seed(context);
        // UserSeeder.Seed(context);
        // LessonCategorySeeder.Seed(context);
        // ListingSeeder.Seed(context);
        // LessonSeeder.Seed(context);
        // ReviewSeeder.Seed(context);
        // ChatSeeder.Seed(context);
        // MessageSeeder.Seed(context);
    }
}
