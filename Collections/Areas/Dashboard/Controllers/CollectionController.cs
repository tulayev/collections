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

        public async Task<IActionResult> Index(string? userId, string sort, string filter, string search, int? page)
        {
            User user;

            if (userId == null)
                user = await _userManager.FindByEmailAsync(User.Identity.Name);
            else
                user = await _userManager.FindByIdAsync(userId);

            ViewData["CurrentSort"] = sort;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sort) ? "name_desc" : "";

            if (search != null)
                page = 1;
            else
                search = filter;

            ViewData["CurrentFilter"] = search;

            var collections = _db.Collections.Where(c => c.UserId == user.Id);

            if (!String.IsNullOrEmpty(search))
                collections = collections.Where(i => i.Name.ToLower().Contains(search.ToLower()));


            switch (sort)
            {
                case "name_desc":
                    collections = collections.OrderByDescending(i => i.Name);
                    break;
                default:
                    collections = collections.OrderBy(i => i.Name);
                    break;
            }

            int perPage = 10;
            return View(await PaginatedList<AppCollection>.CreateAsync(collections.AsNoTracking(), page ?? 1, perPage));
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
