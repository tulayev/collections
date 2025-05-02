using Collections.Models;

namespace Collections.Services.Admin.UserManagement
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<bool> ChangeUserStatusAsync(string userId, UserStatus newStatus);
        Task<bool> DeleteUserAsync(string userId);
    }
}
