using Collections.Models.ViewModels;

namespace Collections.Services.Admin.ProfileManagement
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileAsync(string email);
        Task<bool> UpdateProfileAsync(ProfileViewModel model);
    }
}
