using Amazon.S3;
using Amazon.S3.Model;

namespace Collections.Services
{
    public class S3Handler : IS3Handler
    {
        private readonly IAmazonS3 _s3Client;

        public string BucketName => "t-collections-bucket";

        public S3Handler(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> GetPathAsync(string key)
        {
            bool bucketExists = await _s3Client.DoesS3BucketExistAsync(BucketName);
            
            if (bucketExists)
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = BucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddMonths(1)
                };
                return _s3Client.GetPreSignedURL(urlRequest);
            }

            return String.Empty;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            bool bucketExists = await _s3Client.DoesS3BucketExistAsync(BucketName);

            if (bucketExists)
            {
                var request = new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = file.FileName,
                    InputStream = file.OpenReadStream()
                };

                request.Metadata.Add("Content-Type", file.ContentType);
                await _s3Client.PutObjectAsync(request);
                return file.FileName;
            }

            return String.Empty;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string key)
        {
            bool bucketExists = await _s3Client.DoesS3BucketExistAsync(BucketName);

            if (bucketExists)
            {
                await _s3Client.DeleteObjectAsync(BucketName, key);

                var request = new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = file.FileName,
                    InputStream = file.OpenReadStream()
                };

                request.Metadata.Add("Content-Type", file.ContentType);
                await _s3Client.PutObjectAsync(request);
                return file.FileName;
            }

            return String.Empty;
        }

        public async Task DeleteFileAsync(string key)
        {
            bool bucketExists = await _s3Client.DoesS3BucketExistAsync(BucketName);

            if (bucketExists)
                await _s3Client.DeleteObjectAsync(BucketName, key);
        }
    }
}
