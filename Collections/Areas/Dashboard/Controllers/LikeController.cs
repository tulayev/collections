using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public LikeController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("api/likes-count")]
        public async Task<IActionResult> LikesCount(int itemId)
        {
            return Ok(new 
            { 
                likes_count = await _db.Likes.Where(l => l.Type == Models.Type.Like && l.ItemId == itemId).CountAsync(),
                dislikes_count = await _db.Likes.Where(l => l.Type == Models.Type.Dislike && l.ItemId == itemId).CountAsync()
            });
        }

        [HttpPost]
        [Route("api/like")]
        public async Task<IActionResult> Like(string username, int itemId, int type)
        {
            var user = await _userManager.FindByNameAsync(username);
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (user == null || item == null)
            {
                return Ok(new { error = "Error occured" });
            }

            var existingLike = await _db.Likes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.ItemId == item.Id);

            if (existingLike == null)
            {
                _db.Likes.Add(new Like
                {
                    ItemId = itemId,
                    UserId = user.Id,
                    Type = (Models.Type)type
                });
            }
            else
            {
                if (existingLike.Type != (Models.Type)type)
                {
                    _db.Likes.Remove(existingLike);
                    _db.Likes.Add(new Like
                    {
                        ItemId = itemId,
                        UserId= user.Id,
                        Type = (Models.Type)type
                    });
                }
                else
                {
                    _db.Likes.Remove(existingLike);
                }
            }

            await _db.SaveChangesAsync();
            
            return Ok(new { message = "ok" });
        }
    }
}
