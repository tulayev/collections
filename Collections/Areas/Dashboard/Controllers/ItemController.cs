using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services;
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
        
        private readonly IUploadHandler _uploadHandler;

        public ItemController(ApplicationDbContext db, UserManager<User> userManager, IUploadHandler uploadHandler)
        {
            _db = db;
            _userManager = userManager;
            _uploadHandler = uploadHandler;
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

            string image = String.Empty;    

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image);
            }

            var item = new Item
            {
                Name = model.Name,
                CollectionId = model.CollectionId,
                Tags = tags,
                Image = image.Length > 0 ? image : null
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

            _db.Tags.RemoveRange(item.Tags);

            item.Tags.Clear();

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower() }).ToList();

            string image = String.Empty;

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image, item.Image);
            }

            item.Name = model.Name;
            item.Tags = tags;
            item.Image = image.Length > 0 ? image : null;

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

            _uploadHandler.Delete(item.Image);

            _db.Tags.RemoveRange(item.Tags);

            item.Tags.Clear();

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
