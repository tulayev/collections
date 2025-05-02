using Collections.Models;
using Collections.Models.Dtos;
using Collections.Utils;

namespace Collections.Services.Admin.Likes
{
    public interface ILikeService
    {
        Task<LikeCountDto> GetLikeCountsAsync(int itemId);
        Task<OperationResult> ToggleLikeAsync(string username, int itemId, LikeType type);
    }
}
