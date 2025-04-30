using Collections.Constants;
using Collections.Data;
using Collections.Extensions;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Elastic;
using Collections.Services.Image;
using Collections.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlugGenerator;
using System.Transactions;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;
        private readonly IElasticClientService _elasticClientService;

        public ItemController(
            ApplicationDbContext db, 
            UserManager<User> userManager,
            IImageService imageService,
            IElasticClientService elasticClientService)
        {
            _db = db;
            _userManager = userManager;
            _imageService = imageService;
            _elasticClientService = elasticClientService;
        }

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
            ViewData["CollectionSortParam"] = String.IsNullOrEmpty(sort) ? "collection_desc" : "";

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }
            
            ViewData["CurrentFilter"] = search;

            var userCollections = await _db.Collections.Where(c => c.UserId == user.Id).Select(c => c.Id).ToListAsync();
            
            var userItems = _db.Items
                .Include(i => i.Collection)
                .Include(i => i.File)
                .Where(i => userCollections
                .Contains(i.CollectionId));

            if (!string.IsNullOrEmpty(search))
            {
                userItems = userItems.Where(i => i.Name.ToLower().Contains(search.ToLower())
                                       || i.Collection.Name.ToLower().Contains(search.ToLower()));
            }

            switch (sort)
            {
                case "name_desc":
                    userItems = userItems.OrderByDescending(i => i.Name);
                    break;
                case "collection_desc":
                    userItems = userItems.OrderByDescending(i => i.Collection.Name);
                    break;
                default:
                    userItems = userItems.OrderBy(i => i.Name);
                    break;
            }

            int perPage = 10;

            return View(await PaginatedList<Item>.CreateAsync(userItems.AsNoTracking(), page ?? 1, perPage));
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

            if (collection == null)
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
        public async Task<IActionResult> Create(
            ItemCreateViewModel model, 
            string[] keys, 
            string[] values, 
            int[] types)
        {
            if (keys != null && values != null && keys.Length != values.Length)
            {
                return BadRequest();
            }

            AppFile file = null;    

            if (model.Image != null)
            {
                file = await GetUploadedFile(file, model.Image);
            }

            var item = new Item
            {
                Name = model.Name,
                Slug = String.Empty,
                CollectionId = model.CollectionId,
                Tags = GetTags(model.Tags),
                Fields = GetCustomFields((ItemId: 0, keys, values, types)),
                FileId = file?.Id ?? null,
                CreatedAt = DateTime.Now.SetKindUtc()
            };

            await _db.Items.AddAsync(item);
            
            await _db.SaveChangesAsync();
            
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await _elasticClientService.AddToElasticIndexAsync(new ElasticItemViewModel
            {
                Id = item.Id,
                Item = new ItemDto
                {
                    Slug = item.Slug,
                    CollectionId = item.CollectionId,
                    Name = item.Name,
                    Image = item.File?.Url
                },
                Comments = new List<CommentDto>()
            });

            await _db.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            var itemTags = item.Tags.Select(t => t.Name);

            if (item == null)
            {
                return NotFound();
            }

            var model = new ItemEditViewModel
            {
                Id = item.Id,
                Name = item.Name,
                ExistingImage = item.File?.Url,
                Tags = string.Join(",", itemTags),
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
                .Include(i => i.File)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            var file = item.File;

            if (model.Image != null)
            {
                file = await GetUploadedFile(file, model.Image);
            }

            item.Name = model.Name;
            item.Tags = GetTags(model.Tags, item.Tags);
            item.FileId = file?.Id;
            item.Fields = GetCustomFields((item.Id, keys, values, types), item.Fields);
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await _db.SaveChangesAsync();
            
            await _elasticClientService.UpdateElasticItemAsync(item.Id, new ElasticItemViewModel
            {
                Item = new ItemDto
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    CollectionId = item.CollectionId,
                    Image = item.File?.Url
                }
            });
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound();

            if (item.File != null)
            {
                await _imageService.DeleteImageAsync(item.File.PublicId);
                _db.Files.Remove(item.File);
            }

            _db.Tags.RemoveRange(item.Tags);
            item.Tags.Clear();
            
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            
            await _elasticClientService.RemoveFromElasticIndexAsync<Item>(id);
            
            return RedirectToAction("Index");
        }

        private async Task<AppFile> GetUploadedFile(AppFile file, IFormFile formFile)
        {
            if (file != null)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _imageService.DeleteImageAsync(file.PublicId);

                    var uploadResult = await _imageService.UploadImageAsync(formFile);

                    file.PublicId = uploadResult.PublicId;
                    file.Url = uploadResult.Url.AbsoluteUri;

                    ts.Complete();
                }
            }
            else
            {
                var uploadResult = await _imageService.UploadImageAsync(formFile);

                file = new AppFile
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.AbsoluteUri,
                };

                _db.Files.Add(file);
            }

            await _db.SaveChangesAsync();

            return file;
        }

        private List<Tag> GetTags(string tags, List<Tag> existingTags = null)
        {
            if (existingTags != null)
            {
                _db.Tags.RemoveRange(existingTags);
                existingTags.Clear();
            }
            
            var tagsArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            return tagsArray.ToList().Select(t => new Tag { Name = t.ToLower().Trim() }).ToList();
        }

        private List<Field> GetCustomFields(
            (int ItemId, string[] Keys, string[] Values, int[] Types) data, 
            List<Field> existingFields = null)
        {
            if (existingFields != null)
            {
                _db.Fields.RemoveRange(existingFields);
                existingFields.Clear();
            }

            var fieldsList = new List<Field>();

            for (var i = 0; i < data.Keys.Length; i++)
            {
                fieldsList.Add(new Field
                {
                    Key = data.Keys[i],
                    Value = data.Values[i],
                    ItemId = data.ItemId,
                    Type = (FieldType)data.Types[i]
                });
            }

            if (data.ItemId == 0)
            {
                _db.Fields.AddRange(fieldsList);
            }

            return fieldsList;
        }
    }
}