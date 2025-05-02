using Collections.Constants;
using Collections.Data;
using Collections.Models;
using Collections.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = Roles.RoleAdmin)]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public UserController(
            UserManager<User> userManager, 
            ApplicationDbContext db, 
            IImageService imageService)
        {
            _userManager = userManager;
            _db = db;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserStatus(string userId, int status)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            user.Status = (UserStatus)status;
            
            await _userManager.UpdateSecurityStampAsync(user);
            
            _db.SaveChanges();  
            
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.File)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return BadRequest();
            }

            if (user.File != null)
            {
                _db.Files.Remove(user.File);
                await _imageService.DeleteImageAsync(user.File.PublicId);
            }

            await _db.SaveChangesAsync();   
            
            await _userManager.DeleteAsync(user);
            
            return RedirectToAction("Index");
        }
    }
}
