namespace Collections.Services
{
    public class UploadHandler : IUploadHandler
    {
        private static IWebHostEnvironment _env;
        private const string ImagePath = "images";

        public UploadHandler(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            string fileName = String.Concat(Path.GetRandomFileName(), ext);
            string path = Path.Combine(_env.WebRootPath, ImagePath, fileName);

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
            string path = Path.Combine(_env.WebRootPath, ImagePath, fileName);

            Delete(existingFilePath);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            return fileName;
        }

        public void Delete(string existingFilePath)
        {
            if (!String.IsNullOrWhiteSpace(existingFilePath))
            {
                string oldImagePath = Path.Combine(GetImagesPath, existingFilePath);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
        }

        public static string GetImagesPath => Path.Combine(_env.WebRootPath, ImagePath);
    }
}
