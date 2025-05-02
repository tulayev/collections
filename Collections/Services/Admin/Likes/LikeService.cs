using Collections.Data.Repositories;
using Collections.Models.Dtos;
using Collections.Models;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;

namespace Collections.Services.Admin.Likes
{
    public class LikeService : ILikeService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public LikeService(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<LikeCountDto> GetLikeCountsAsync(int itemId)
        {
            var repo = _unitOfWork.Repository<Like>();

            var likes = await repo.FindAsync(l => l.ItemId == itemId && l.Type == LikeType.Like);
            var dislikes = await repo.FindAsync(l => l.ItemId == itemId && l.Type == LikeType.Dislike);

            return new LikeCountDto
            {
                LikesCount = likes.Count(),
                DislikesCount = dislikes.Count()
            };
        }

        public async Task<OperationResult> ToggleLikeAsync(string username, int itemId, LikeType type)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return OperationResult.Fail("User not found");
            }

            var repo = _unitOfWork.Repository<Like>();
            var existing = (await repo
                .FindAsync(l => l.UserId == user.Id && l.ItemId == itemId))
                .FirstOrDefault();

            if (existing == null)
            {
                await repo.AddAsync(new Like
                {
                    UserId = user.Id,
                    ItemId = itemId,
                    Type = type
                });
            }
            else if (existing.Type != type)
            {
                repo.Remove(existing);
                await repo.AddAsync(new Like
                {
                    UserId = user.Id,
                    ItemId = itemId,
                    Type = type
                });
            }
            else
            {
                repo.Remove(existing);
            }

            await _unitOfWork.SaveChangesAsync();

            return OperationResult.Ok();
        }
    }
}
