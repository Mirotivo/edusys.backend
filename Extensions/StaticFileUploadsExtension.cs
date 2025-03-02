using Microsoft.Extensions.FileProviders;

public static class StaticFileUploadsExtension
{
    public static IApplicationBuilder UseStaticFilesUploads(this IApplicationBuilder app)
    {
        // Define the uploads directory
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Ensure the directory exists
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        // Configure static file serving for uploads
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/api/uploads",
            OnPrepareResponse = ctx =>
            {
                Console.WriteLine($"Serving file: {ctx.File.PhysicalPath}");
            }
        });

        return app;
    }
}
