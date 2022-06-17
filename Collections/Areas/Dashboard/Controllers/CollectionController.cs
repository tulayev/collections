using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;

        public CollectionController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? userId)
        {
            User user;

            if (userId == null)
            {
                user = await _userManager.FindByEmailAsync(User.Identity.Name);
            }
            else
            {
                user = await _userManager.FindByIdAsync(userId);    
            }

            var collections = await _db.Collections.Where(c => c.UserId == user.Id).ToListAsync();
            
            return View(collections);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppCollection model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            model.UserId = user.Id;

            await _db.Collections.AddAsync(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id);

            if (collection is null)
            {
                return NotFound();
            }

            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppCollection model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.Collections.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id);

            if (collection is null)
            {
                return NotFound();
            }

            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
