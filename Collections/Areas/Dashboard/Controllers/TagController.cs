using Collections.Models;
using Collections.Services.Admin.Tags;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<List<Tag>> GetAll()
        {
            return await _tagService.GetAllTagsAsync();
        }
    }
}
