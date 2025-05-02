using Collections.Constants;
using Collections.Data.Repositories;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Image;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Collections.Services.Account
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IImageService imageService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            Claim imageClaim = null;
            AppFile file = null;

            if (model.Image != null)
            {
                var uploadResult = await _imageService.UploadImageAsync(model.Image);

                file = new AppFile
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.AbsoluteUri
                };

                await _unitOfWork.Repository<AppFile>().AddAsync(file);
                await _unitOfWork.SaveChangesAsync();

                imageClaim = new Claim("Image", file.Url);
            }

            var user = new User
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                FileId = file?.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (!result.Succeeded)
            {
                return result;
            }

            if (imageClaim != null)
            {
                await _userManager.AddClaimAsync(user, imageClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim("Name", user.Name));

            var userRole = await _roleManager.FindByNameAsync(Roles.RoleUser);

            if (userRole != null)
            {
                await _userManager.AddToRoleAsync(user, userRole.Name);
            }

            return IdentityResult.Success;
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return result;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user.Status != UserStatus.Default)
            {
                await _signInManager.SignOutAsync();
                return SignInResult.Failed;
            }

            return result;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
