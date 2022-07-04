using Collections.Data;
using Collections.Models;
using Collections.Utils;
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

        public UserController(UserManager<User> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;   
        }

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

            user.Status = (Status)status;
            await _userManager.UpdateSecurityStampAsync(user);
            _db.SaveChanges();  
            return RedirectToAction("Index");
        }
    }
}
