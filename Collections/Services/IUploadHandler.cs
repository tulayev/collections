namespace Collections.Services
{
    public interface IUploadHandler
    {
        Task<string> UploadAsync(IFormFile file);

        Task<string> UploadAsync(IFormFile file, string existingFilePath);
    }
}
