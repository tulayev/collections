using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Elastic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.Comments
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IElasticClientService _elasticClientService;

        public CommentService(
            ApplicationDbContext db,
            UserManager<User> userManager,
            IElasticClientService elasticClientService)
        {
            _db = db;
            _userManager = userManager;
            _elasticClientService = elasticClientService;
        }

        public async Task<List<CommentViewModel>> GetCommentsAsync(int itemId)
        {
            return await _db.Comments
                .Include(c => c.User)
                    .ThenInclude(u => u.File)
                .Where(c => c.ItemId == itemId)
                .Select(c => new CommentViewModel
                {
                    Body = c.Body,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    User = new CommentUserViewModel
                    {
                        Name = c.User.Name,
                        Image = c.User.File.Url
                    }
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> PostCommentAsync(string username, int itemId, string body)
        {
            var user = await _userManager.FindByNameAsync(username);
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (user == null || item == null)
            {
                return (false, "Error occurred");
            }

            var comment = new Comment
            {
                Body = body,
                UserId = user.Id,
                ItemId = itemId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var commentDtos = await _db.Comments
                .Where(c => c.ItemId == itemId)
                .Select(c => new CommentDto { Body = c.Body })
                .ToListAsync();

            await _elasticClientService.UpdateElasticItemAsync(itemId, new ElasticItemViewModel
            {
                Comments = commentDtos
            });

            return (true, "ok");
        }
    }
}
