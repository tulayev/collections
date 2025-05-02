using Collections.Models.ViewModels;

namespace Collections.Services.Admin.Roles
{
    public interface IRoleService
    {
        Task<RoleEditViewModel> GetEditModelAsync(string userId);
        Task<bool> UpdateUserRolesAsync(string userId, IList<string> newRoles);
    }
}
