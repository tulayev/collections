using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    public class LikeController : Controller
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
        public async Task<IActionResult> LikesCount()
        {
            return Json(new { likes = await _db.Likes.CountAsync() });
        }

        [HttpPost]
        [Route("api/like")]
        public async Task<IActionResult> Like(string username, int itemId)
        {
            var user = await _userManager.FindByNameAsync(username);
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (user == null || item == null)
            {
                return Json(new { Error = "error occured" });
            }

            var like = new Like
            {
                ItemId = itemId,
                UserId = user.Id
            };

            if (!_db.Likes.Contains(like))
            {
                _db.Likes.Add(like);
            }
            else
            {
                _db.Likes.Remove(like);
            }

            await _db.SaveChangesAsync();
            return Json(new { message = "ok" });
        }
    }
}
