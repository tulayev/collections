using Collections.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Collections.Services.Account
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
    }
}
