using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;

        public ItemController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            var userCollections = await _db.Collections.Where(c => c.UserId == user.Id).Select(c => c.Id).ToListAsync();

            var userItems = await _db.Items.Where(i => userCollections.Contains(i.CollectionId)).ToListAsync();

            return View(userItems);
        }
        
        public async Task<IActionResult> Create(int? collection)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            if (collection is null)
			{
                return NotFound();
			}

            var userCollection = await _db.Collections.FindAsync(collection);

            if (userCollection != null && userCollection.UserId != user.Id)
			{
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower() }).ToList();

            var item = new Item
            {
                Name = model.Name,
                CollectionId = model.CollectionId,
                Tags = tags
            };

            await _db.Items.AddAsync(item);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == id);
            var itemTags = item.Tags.Select(t => t.Name);

            if (item is null)
            {
                return NotFound();
            }

            var model = new ItemEditViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Tags = String.Join(", ", itemTags)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = await _db.Items.Include(i => i.Tags).FirstOrDefaultAsync(i => i.Id == model.Id);

            item.Tags.Clear();

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower() }).ToList();

            item.Name = model.Name;
            item.Tags = tags;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Items.Include(i => i.Tags).FirstOrDefaultAsync(i => i.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            item.Tags.Clear();

            _db.Items.Remove(item);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
