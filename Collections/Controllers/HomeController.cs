using Collections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string tag)
        {
            if (tag != null)
            {
                var itemsByTag = await _db.Items
                    .Include(i => i.Fields)
                    .Include(i => i.Tags)
                    .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                return View(itemsByTag);
            }

            var items = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.Fields)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        public IActionResult Show(int id)
        {
            return View();
        }
    }
}