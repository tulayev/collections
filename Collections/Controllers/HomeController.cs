using Collections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Collections.Utils;
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

        public async Task<IActionResult> Index(string? tag, int page = 1)
        {
            int count = 0;
            int perPage = 12;
            PageViewModel pageModel;

            var source = _db.Items
                .AsNoTracking()
                .Include(i => i.Tags)
                .Include(i => i.Fields)
                .Include(i => i.Collection);

            if (tag != null)
            {
                var itemsByTag = await source
                    .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                count = itemsByTag.Count;

                pageModel = new PageViewModel(count, page, perPage);

                return View(new IndexViewModel
                {
                    Items = itemsByTag,
                    PageViewModel = pageModel
                });
            }

            count = await source.CountAsync();
            
            var items = await source
                .OrderByDescending(i => i.CreatedAt)
                .Paginate(page, perPage)
                .ToListAsync();

            pageModel = new PageViewModel(count, page, perPage);

            var model = new IndexViewModel
            {
                Items = items,
                PageViewModel = pageModel
            };

            return View(model);
        }

        public async Task<IActionResult> Show(string slug)
        {
            var item = await _db.Items
                .Include(i => i.Fields)
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