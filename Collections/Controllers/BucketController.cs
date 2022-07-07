using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    [Route("api/buckets")]
    [ApiController]
    public class BucketController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        
        public BucketController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBucketAsync()
        {
            var data = await _s3Client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => { return b.BucketName; });
            return Ok(buckets);
        }
    }
}
