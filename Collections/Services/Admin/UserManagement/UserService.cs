using Collections.Data.Repositories;
using Collections.Models;
using Collections.Services.Image;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.UserManagement
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            UserManager<User> userManager,
            IImageService imageService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userManager.Users
                .Include(u => u.File)
                .ToListAsync();
        }

        public async Task<bool> ChangeUserStatusAsync(string userId, UserStatus newStatus)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return false;
            }

            user.Status = newStatus;
            
            await _userManager.UpdateSecurityStampAsync(user);

            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.File)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            if (user.File != null)
            {
                _unitOfWork.Repository<AppFile>().Remove(user.File);
                await _imageService.DeleteImageAsync(user.File.PublicId);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            
            return result.Succeeded;
        }
    }
}
