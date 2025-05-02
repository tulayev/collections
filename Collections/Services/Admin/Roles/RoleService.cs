using Collections.Models.ViewModels;
using Collections.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.Roles
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<RoleEditViewModel> GetEditModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            return new RoleEditViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserRoles = userRoles,
                AllRoles = allRoles
            };
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, IList<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var toAdd = newRoles.Except(currentRoles);
            var toRemove = currentRoles.Except(newRoles);

            if (toAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);

                if (!addResult.Succeeded)
                {
                    return false;
                }
            }

            if (toRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);

                if (!removeResult.Succeeded)
                {
                    return false;
                }
            }

            await _userManager.UpdateSecurityStampAsync(user);

            return true;
        }
    }
}
