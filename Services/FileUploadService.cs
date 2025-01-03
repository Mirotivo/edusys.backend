using Microsoft.Extensions.Options;

public class FileUploadService : IFileUploadService
{
    private readonly string _baseUploadFolder;
    private readonly AppOptions _appOptions;

    public FileUploadService(
        IOptions<AppOptions> appOptions
    )
    {
        _baseUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        _appOptions = appOptions.Value;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory = "")
    {
        var uploadsFolder = string.IsNullOrEmpty(subDirectory)
            ? _baseUploadFolder
            : Path.Combine(_baseUploadFolder, subDirectory);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate a unique file name (using GUID)
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"{_appOptions.BaseUrl}/uploads/{(string.IsNullOrEmpty(subDirectory) ? "" : $"{subDirectory}/")}{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var fullPath = Path.Combine(_baseUploadFolder, Path.GetFileName(filePath));

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
        }
    }

    public async Task<string?> UpdateFileAsync(IFormFile? newFile, string? currentFilePath, string subDirectory = "")
    {
        if (newFile == null)
        {
            return currentFilePath;
        }

        // Delete the current file if it exists
        if (!string.IsNullOrEmpty(currentFilePath))
        {
            await DeleteFileAsync(currentFilePath);
        }

        // Save the new file
        return await SaveFileAsync(newFile, subDirectory);
    }
}
