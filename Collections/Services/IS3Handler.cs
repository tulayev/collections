namespace Collections.Services
{
    public interface IS3Handler
    {
        string BucketName { get; }

        Task<string> GetPathAsync(string key);
        
        Task<string> UploadFileAsync(IFormFile file);

        Task<string> UploadFileAsync(IFormFile file, string key);
        
        Task DeleteFileAsync(string key);

    }
}
