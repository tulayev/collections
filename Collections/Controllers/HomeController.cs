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
            if (tag != null)
            {
                var itemsByTag = await _db.Items
                    .AsNoTracking()
                    .Include(i => i.Fields)
                    .Include(i => i.Tags)
                    .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                return View(itemsByTag);
            }

            var source = _db.Items
                .AsNoTracking()
                .Include(i => i.Tags)
                .Include(i => i.Fields);

            int count = await source.CountAsync();
            int perPage = 12;
            
            var items = await source
                .OrderByDescending(i => i.CreatedAt)
                .Paginate(page, perPage)
                .ToListAsync();

            var pageModel = new PageViewModel(count, page, perPage);

            var model = new IndexViewModel
            {
                Items = items,
                PageViewModel = pageModel
            };

            return View(model);
        }

        public IActionResult Show(int id)
        {
            return View();
        }
    }
}