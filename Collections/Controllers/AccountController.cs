using Collections.Constants;
using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Image;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Collections.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IImageService _imageService;

        public AccountController(
            ApplicationDbContext db, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<IdentityRole> roleManager,
            IImageService imageService)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
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

                _db.Files.Add(file);    
                
                await _db.SaveChangesAsync();
                
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

            if (result.Succeeded)
            {
                if (imageClaim != null)
                {
                    await _userManager.AddClaimAsync(user, imageClaim);
                }
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new("Name", user.Name)
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

        [HttpGet]
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
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user.Status == Status.Default)
                    return RedirectToAction("Index", "Home");
                else
                {
                    ModelState.AddModelError("Login", "Failed to login! The user has been blocked");
                    await _signInManager.SignOutAsync();
                    return View(model);
                }

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
