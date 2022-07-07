namespace Collections.Services
{
    public interface IFileHandler
    {
        Task<string> UploadAsync(IFormFile file);

        Task<string> UploadAsync(IFormFile file, string existingFilePath);

        string GeneratePath(string filename);
    }
}
