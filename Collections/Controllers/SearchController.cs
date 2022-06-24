using Collections.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace Collections.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticClient _client;

        public SearchController(IElasticClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string keyword)
        {
            var results = await _client.SearchAsync<ElasticItemViewModel>(
                s => s.Query(
                    q => q.QueryString(
                        d => d.Query('*' + keyword + '*')
                    )
                ).Size(1000)
            );

            return Ok(results.Documents.ToList());
        }

        [Route("test/{id:int}")]
        [HttpGet]
        public void Remove(int id)
        {
            _client.Delete<ElasticItemViewModel>(id);
        }
    }
}
