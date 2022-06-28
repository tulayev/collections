using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services;
using Collections.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using SlugGenerator;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;
        
        private readonly IUploadHandler _uploadHandler;

        private readonly IElasticClient _client;

        public ItemController(ApplicationDbContext db, UserManager<User> userManager, 
            IUploadHandler uploadHandler, IElasticClient client)
        {
            _db = db;
            _userManager = userManager;
            _uploadHandler = uploadHandler;
            _client = client;
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

            var userCollections = await _db.Collections.Where(c => c.UserId == user.Id).Select(c => c.Id).ToListAsync();
            var userItems = await _db.Items.Where(i => userCollections.Contains(i.CollectionId)).ToListAsync();

            return View(userItems);
        }
        
        public async Task<IActionResult> Create(int? collection)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var adminUsers = await _userManager.GetUsersInRoleAsync(Roles.RoleAdmin);
            var adminUsersList = new List<User>();

            foreach (var admin in adminUsers)
            {
                adminUsersList.Add(admin);
            }

            if (collection is null)
			{
                return NotFound();
			}

            var userCollection = await _db.Collections.FindAsync(collection);

            if (userCollection == null || 
                (userCollection.UserId != user.Id && !adminUsersList.Select(a => a.Id).Contains(user.Id)))
			{
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateViewModel model, string[] keys, string[] values, int[] types)
        {
            if (keys != null && values != null && keys.Length != values.Length)
            {
                return BadRequest();
            }

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower().Trim() }).ToList();

            string image = String.Empty;    

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image);
            }

            var item = new Item
            {
                Name = model.Name,
                Slug = String.Empty,
                CollectionId = model.CollectionId,
                Tags = tags,
                Image = image,
                CreatedAt = DateTime.Now.SetKindUtc()
            };

            await _db.Items.AddAsync(item);
            await _db.SaveChangesAsync();
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await AddToElasticIndex(item);

            var fieldsList = new List<Models.Field>();

            for (int i = 0; i < keys.Length; i++)
            {
                fieldsList.Add(new Models.Field 
                {
                    Key = keys[i],
                    Value = values[i],
                    ItemId = item.Id,
                    Type = (Models.FieldType)types[i]
                });
            }

            await _db.Fields.AddRangeAsync(fieldsList);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.Fields)
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
                ExistingImage = item.Image,
                Tags = String.Join(",", itemTags),
                Fields = item.Fields
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemEditViewModel model, string[] keys, string[] values, int[] types)
        {
            if (keys != null && values != null && keys.Length != values.Length)
            {
                return BadRequest();
            }

            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            _db.Tags.RemoveRange(item.Tags);
            item.Tags.Clear();

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower().Trim() }).ToList();

            string image = String.Empty;

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image, item.Image);
            }

            _db.Fields.RemoveRange(item.Fields);
            item.Fields.Clear();

            var fieldsList = new List<Models.Field>();

            for (int i = 0; i < keys.Length; i++)
            {
                fieldsList.Add(new Models.Field
                {
                    Key = keys[i],
                    Value = values[i],
                    ItemId = item.Id,
                    Type = (Models.FieldType)types[i]
                });
            }

            item.Name = model.Name;
            item.Tags = tags;
            item.Image = image.Length > 0 ? image : item.Image;
            item.Fields = fieldsList;
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await _db.SaveChangesAsync();
            await UpdateElasticIndex(item);
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
            await RemoveFromElasticIndex(id);
            return RedirectToAction("Index");
        }

        private async Task AddToElasticIndex(Item item)
        {
            var elasticItem = new ElasticItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Slug = item.Slug,
                CollectionId = item.CollectionId,
                Image = item.Image
            };

            await _client.IndexDocumentAsync(elasticItem);
        }

        private async Task UpdateElasticIndex(Item item) =>
            await _client.UpdateAsync<ElasticItemViewModel>(
                item.Id,
                u => u.Index("items").Doc(new ElasticItemViewModel 
                { 
                    Name = item.Name, 
                    CollectionId = item.CollectionId, 
                    Slug = item.Slug,
                    Image = item.Image
                })
            );

        private async Task RemoveFromElasticIndex(int id) => await _client.DeleteAsync<ElasticItemViewModel>(id);
    }
}
