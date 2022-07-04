using Collections.Data;
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
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;
        
        private readonly SignInManager<User> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IFileHandler _fileHandler;

        public AccountController(ApplicationDbContext db, UserManager<User> userManager, SignInManager<User> signInManager, 
            RoleManager<IdentityRole> roleManager, IFileHandler fileHandler)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _fileHandler = fileHandler;
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

            Claim imageClaim = null;
            AppFile file = null;

            if (model.Image != null)
            {
                string filename = await _fileHandler.UploadAsync(model.Image);
                file = new AppFile
                {
                    Name = filename,
                    Path = _fileHandler.GeneratePath(filename)
                };
                _db.Files.Add(file);    
                await _db.SaveChangesAsync();
                imageClaim = new Claim("Image", file.Name);
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
