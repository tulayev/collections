using Collections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Collections.Utils;
using Collections.Models;
using Collections.Models.ViewModels;

namespace Collections.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tag, int? collection, string user, int? page)
        {
            var source = _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .Include(i => i.Collection)
                    .ThenInclude(i => i.User);

            var collections = _db.Collections
                .Include(c => c.User)
                .Include(c => c.Items)
                .GroupBy(c => new { Id = c.Id, Name = c.Name, Author = c.User.Name, Total = c.Items.Count })
                .Select(c => new AppCollectionViewModel { Id = c.Key.Id, Name = c.Key.Name, Author = c.Key.Author, Total = c.Key.Total })
                .OrderByDescending(x => x.Total)
                .Take(5);

            const int perPage = 12;

            if (tag != null)
            {
                var itemsByTag = source
                    .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                    .OrderByDescending(i => i.CreatedAt);

                return View(new HomePageViewModel
                {
                    Items = await PaginatedList<Item>.CreateAsync(itemsByTag.AsNoTracking(), page ?? 1, perPage),
                    Collections = await collections.ToListAsync()
                });
            }

            if (user != null && collection != null)
            {
                var itemsByCollection = source
                    .Where(i => i.Collection.Id == collection && i.Collection.User.Name == user)
                    .OrderByDescending(i => i.CreatedAt);

                return View(new HomePageViewModel
                {
                    Items = await PaginatedList<Item>.CreateAsync(itemsByCollection.AsNoTracking(), page ?? 1, perPage),
                    Collections = await collections.ToListAsync()
                });
            }

            var items = source
                .OrderByDescending(i => i.CreatedAt);

            return View(new HomePageViewModel
            {
                Items = await PaginatedList<Item>.CreateAsync(items.AsNoTracking(), page ?? 1, perPage),
                Collections = await collections.ToListAsync()
            });
        }

        [HttpGet]
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