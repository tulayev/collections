using Collections.Models;
using Collections.Services.Admin.Likes;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpGet("api/likes-count")]
        public async Task<IActionResult> LikesCount(int itemId)
        {
            var dto = await _likeService.GetLikeCountsAsync(itemId);

            return Ok(new
            {
                likes_count = dto.LikesCount,
                dislikes_count = dto.DislikesCount
            });
        }

        [HttpPost("api/like")]
        public async Task<IActionResult> Like(string username, int itemId, int type)
        {
            var result = await _likeService.ToggleLikeAsync(username, itemId, (LikeType)type);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "ok" });
        }
    }
}
