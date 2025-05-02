using Collections.Models.ViewModels;
using Collections.Services.Elastic;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticClientService _elasticClientService;

        public SearchController(IElasticClientService elasticClientService)
        {
            _elasticClientService = elasticClientService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string keyword)
        {
            var results = await _elasticClientService.SearchAsync<ElasticItemViewModel>(keyword);

            return Ok(results.Documents.ToList());
        }

        [Route("test")]
        [HttpGet]
        public async Task Remove(int id)
        {
            await _elasticClientService.RemoveFromElasticIndexAsync<ElasticItemViewModel>(id);
        }
    }
}
