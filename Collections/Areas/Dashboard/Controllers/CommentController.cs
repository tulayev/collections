using Collections.Services.Admin.Comments;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int itemId)
        {
            var comments = await _commentService.GetCommentsAsync(itemId);
            return Ok(new { comments });
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post(string username, int itemId, string body)
        {
            var (success, message) = await _commentService.PostCommentAsync(username, itemId, body);

            if (!success)
            {
                return Ok(new { error = message });
            }

            return Ok(new { message });
        }
    }
}
