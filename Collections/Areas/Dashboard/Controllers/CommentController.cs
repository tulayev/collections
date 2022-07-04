using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;

        public CommentController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int itemId) => Ok(new
        {
            comments = await _db.Comments
                .Include(c => c.User)
                    .ThenInclude(u => u.File)
                .Where(c => c.ItemId == itemId)
                .Select(c => new CommentViewModel
                {
                    Body = c.Body,
                    User = new CommentUserViewModel { Name = c.User.Name, Image = c.User.File.Name },
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync()
        });

        [HttpPost("post")]
        public async Task<IActionResult> Post(string username, int itemId, string body)
        {
            var user = await _userManager.FindByNameAsync(username);
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (user == null || item == null)
            {
                return Ok(new { error = "Error occured" });
            }

            var comment = new Comment
            {
                Body = body,
                UserId = user.Id,
                ItemId = item.Id,
                CreatedAt = DateTime.Now.SetKindUtc()
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return Ok(new { message = "ok" });
        }
    }
}
