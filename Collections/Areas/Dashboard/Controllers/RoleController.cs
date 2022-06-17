using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Admin.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = Roles.RoleAdmin)]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<User> _userManager;
        
        private readonly SignInManager<User> _signManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signManager = signManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                var rolesToBeRemoved = await _userManager.GetRolesAsync(user);

                await _userManager.AddToRoleAsync(user, roles[0]);

                await _userManager.RemoveFromRoleAsync(user, rolesToBeRemoved[0]);

                if (user.Email == User.Identity.Name)
                {
                    await _signManager.SignOutAsync();
                }

                return RedirectToAction("Index", "User");
            }

            return NotFound();
        }
    }
}
