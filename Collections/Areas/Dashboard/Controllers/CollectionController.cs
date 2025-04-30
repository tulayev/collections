using Collections.Data;
using Collections.Models;
using Collections.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _env;

        public CollectionController(ApplicationDbContext db, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userId, string sort, string filter, string search, int? page)
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

            ViewData["CurrentSort"] = sort;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sort) ? "name_desc" : "";

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }

            ViewData["CurrentFilter"] = search;

            var collections = _db.Collections.Include(c => c.User).Where(c => c.UserId == user.Id);

            if (!string.IsNullOrEmpty(search))
            {
                collections = collections.Where(i => i.Name.ToLower().Contains(search.ToLower()));
            }

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

        [HttpGet]
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

        [HttpGet]
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

        [HttpGet]
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

        [HttpGet]
        public async Task<IActionResult> Export(string userId)
        {
            List<AppCollection> collections = null;

            var sb = new StringBuilder();
            var filename = "collections.csv";
            var path = Path.Combine(new string[] { _env.WebRootPath, "uploads", filename });

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            if (userId != null)
            {
                collections = await _db.Collections.Where(c => c.UserId == userId).ToListAsync();
            }

            if (collections != null)
            {
                sb.AppendLine("ID,Name");
                
                foreach (var collection in collections)
                {
                    string newLine = $"{collection.Id},{collection.Name}";
                    sb.AppendLine(newLine);
                }

                System.IO.File.WriteAllText(path, sb.ToString());

                return File(System.IO.File.ReadAllBytes(path), "application/octet-stream", filename);
            }

            return RedirectToAction("Index");
        }
    }
}
