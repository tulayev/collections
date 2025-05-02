using Collections.Data.Repositories;
using Collections.Models.ViewModels;
using Collections.Models;
using Collections.Services.Image;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Collections.Services.Admin.ProfileManagement
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(
            UserManager<User> userManager,
            IImageService imageService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProfileViewModel> GetProfileAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                       ?? throw new KeyNotFoundException($"User with email '{email}' not found");

            return new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<bool> UpdateProfileAsync(ProfileViewModel model)
        {
            var user = await _userManager.Users
                .Include(u => u.File)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null)
            {
                return false;
            }

            AppFile file = user.File;
            Claim imageClaim = null;

            if (model.Image != null)
            {
                if (file != null)
                {
                    using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    await _imageService.DeleteImageAsync(file.PublicId);

                    var uploadResult = await _imageService.UploadImageAsync(model.Image);

                    file.PublicId = uploadResult.PublicId;
                    file.Url = uploadResult.Url.AbsoluteUri;

                    _unitOfWork.Repository<AppFile>().Update(file);

                    ts.Complete();
                }
                else
                {
                    var uploadResult = await _imageService.UploadImageAsync(model.Image);
                    
                    file = new AppFile
                    {
                        PublicId = uploadResult.PublicId,
                        Url = uploadResult.Url.AbsoluteUri
                    };
                    
                    await _unitOfWork.Repository<AppFile>().AddAsync(file);
                    await _unitOfWork.SaveChangesAsync(); 
                    
                    user.FileId = file.Id;
                }

                imageClaim = new Claim("Image", file.Url);
            }

            user.Name = model.Name ?? user.Name;
            user.Email = model.Email ?? user.Email;
            user.UserName = model.Email ?? user.UserName;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            }

            var claims = await _userManager.GetClaimsAsync(user);

            if (imageClaim != null)
            {
                var old = claims.FirstOrDefault(c => c.Type == "Image");

                if (old != null)
                {
                    await _userManager.ReplaceClaimAsync(user, old, imageClaim);
                }
                else
                {
                    await _userManager.AddClaimAsync(user, imageClaim);
                }
            }

            var nameClaim = claims.FirstOrDefault(c => c.Type == "Name");

            if (nameClaim != null)
            {
                await _userManager.ReplaceClaimAsync(user, nameClaim, new Claim("Name", user.Name));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new Claim("Name", user.Name));
            }

            await _userManager.UpdateSecurityStampAsync(user);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
