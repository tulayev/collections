using Collections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Collections.Utils;
using Collections.Models;

namespace Collections.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? tag, int? page)
        {
            var source = _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .Include(i => i.Collection);

            int perPage = 12;

            if (tag != null)
            {
                var itemsByTag = source
                    .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                    .OrderByDescending(i => i.CreatedAt);

                return View(await PaginatedList<Item>.CreateAsync(itemsByTag.AsNoTracking(), page ?? 1, perPage));
            }

            var items = source
                .OrderByDescending(i => i.CreatedAt);

            return View(await PaginatedList<Item>.CreateAsync(items.AsNoTracking(), page ?? 1, perPage));
        }

        public async Task<IActionResult> Show(string slug)
        {
            var item = await _db.Items
                .Include(i => i.Fields)
                .Include(i => i.File)
                .Include(i => i.Collection)
                .FirstOrDefaultAsync(i => i.Slug == slug);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }
    }
}