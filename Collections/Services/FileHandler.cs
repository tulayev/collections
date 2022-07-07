namespace Collections.Services
{
    public class FileHandler : IFileHandler
    {
        private readonly IWebHostEnvironment _env;
        
        private const string UploadsPath = "uploads";

        public FileHandler(IWebHostEnvironment env)
        {
            _env = env;

            if (!Directory.Exists(Path.Combine(_env.WebRootPath, UploadsPath)))
            {
                Directory.CreateDirectory(Path.Combine(_env.WebRootPath, UploadsPath));
            }
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            string fileName = String.Concat(Path.GetRandomFileName(), ext);
            string path = GeneratePath(fileName);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            return fileName;
        }

        public async Task<string> UploadAsync(IFormFile file, string existingFilePath)
        {
            string ext = Path.GetExtension(file.FileName);
            string fileName = String.Concat(Path.GetRandomFileName(), ext);
            string path = GeneratePath(fileName);

            Delete(existingFilePath);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            return fileName;
        }

        public string GeneratePath(string filename) => Path.Combine(new string[] { _env.WebRootPath, UploadsPath, filename });

        private void Delete(string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
