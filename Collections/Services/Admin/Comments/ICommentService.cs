using Collections.Models.ViewModels;

namespace Collections.Services.Admin.Comments
{
    public interface ICommentService
    {
        Task<List<CommentViewModel>> GetCommentsAsync(int itemId);
        Task<(bool Success, string Message)> PostCommentAsync(string username, int itemId, string body);
    }
}
