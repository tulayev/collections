using Collections.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Collections.Services.Account
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
    }
}
