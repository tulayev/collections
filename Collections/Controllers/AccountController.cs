using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Collections.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        
        private readonly SignInManager<User> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IUploadHandler _uploadHandler;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            RoleManager<IdentityRole> roleManager, IUploadHandler uploadHandler)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _uploadHandler = uploadHandler;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string image = String.Empty;
            Claim imageClaim = null;

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image);
                imageClaim = new Claim("Image", image);
            }

            var user = new User
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                Image = image
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (imageClaim != null)
                {
                    await _userManager.AddClaimAsync(user, imageClaim);
                }
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim("Name", user.Name)
                });

                var userRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == Roles.RoleUser);
                await _userManager.AddToRoleAsync(user, userRole.Name);
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Login", "Failed to login!");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
