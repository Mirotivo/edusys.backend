public interface IFileUploadService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory = "");
    Task DeleteFileAsync(string filePath);
    Task<string?> UpdateFileAsync(IFormFile? newFile, string? currentFilePath, string subDirectory = "");
}
