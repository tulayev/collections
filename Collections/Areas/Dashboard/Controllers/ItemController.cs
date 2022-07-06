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
        
        private readonly IFileHandler _fileHandler;

        private readonly AppElasticClient _elasticClient;

        public ItemController(ApplicationDbContext db, UserManager<User> userManager, 
            IFileHandler fileHandler, IElasticClient client)
        {
            _db = db;
            _userManager = userManager;
            _fileHandler = fileHandler;
            _elasticClient = new AppElasticClient(client);
        }

        public async Task<IActionResult> Index(string? userId, string sort, string filter, string search, int? page)
        {
            User user;

            if (userId == null)
                user = await _userManager.FindByEmailAsync(User.Identity.Name);
            else
                user = await _userManager.FindByIdAsync(userId);

            ViewData["CurrentSort"] = sort;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sort) ? "name_desc" : "";
            ViewData["CollectionSortParam"] = String.IsNullOrEmpty(sort) ? "collection_desc" : "";

            if (search != null)
                page = 1;
            else
                search = filter;
            
            ViewData["CurrentFilter"] = search;

            var userCollections = await _db.Collections.Where(c => c.UserId == user.Id).Select(c => c.Id).ToListAsync();
            var userItems = _db.Items
                .Include(i => i.Collection)
                .Include(i => i.File)
                .Where(i => userCollections
                .Contains(i.CollectionId));

            if (!String.IsNullOrEmpty(search))
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

            int perPage = 3;
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

            AppFile file = null;    

            if (model.Image != null)
            {
                string filename = await _fileHandler.UploadAsync(model.Image);
                file = new AppFile
                {
                    Name = filename,
                    Path = _fileHandler.GeneratePath(filename)
                };
                _db.Files.Add(file);
                await _db.SaveChangesAsync();
            }

            var item = new Item
            {
                Name = model.Name,
                Slug = String.Empty,
                CollectionId = model.CollectionId,
                Tags = tags,
                FileId = file?.Id ?? null,
                CreatedAt = DateTime.Now.SetKindUtc()
            };

            await _db.Items.AddAsync(item);
            await _db.SaveChangesAsync();
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await _elasticClient.AddToElasticIndex(new ElasticItemViewModel
            {
                Id = item.Id,
                Item = new ItemDto
                {
                    Slug = item.Slug,
                    CollectionId = item.CollectionId,
                    Name = item.Name,
                    Image = item.File?.Name
                },
                Comments = new List<CommentDto>()
            });

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
                .Include(i => i.File)
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
                ExistingImage = item.File?.Name,
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
                .Include(i => i.File)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            _db.Tags.RemoveRange(item.Tags);
            item.Tags.Clear();

            string[] tagsArray = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var tags = tagsArray.ToList().Select(t => new Tag { Name = t.ToLower().Trim() }).ToList();

            var file = item.File;

            if (model.Image != null)
            {
                if (file != null)
                {
                    string filename = await _fileHandler.UploadAsync(model.Image, file.Path);
                    file.Name = filename;
                    file.Path = _fileHandler.GeneratePath(filename);
                }
                else
                {
                    string filename = await _fileHandler.UploadAsync(model.Image);
                    file.Name = filename;
                    file.Path = _fileHandler.GeneratePath(filename);
                    _db.Files.Add(file);
                }
                await _db.SaveChangesAsync();
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
            item.FileId = file?.Id;
            item.Fields = fieldsList;
            item.Slug = $"{item.Id}-{item.Name.GenerateSlug()}";

            await _db.SaveChangesAsync();
            
            await _elasticClient.UpdateElasticItem(item.Id, new ElasticItemViewModel
            {
                Item = new ItemDto
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    CollectionId = item.CollectionId,
                    Image = item.File?.Name
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

            if (item is null)
            {
                return NotFound();
            }

            if (item.File != null)
                _db.Files.Remove(item.File);
            _db.Tags.RemoveRange(item.Tags);
            item.Tags.Clear();
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            
            await _elasticClient.RemoveFromElasticIndex(id);
            
            return RedirectToAction("Index");
        }
    }
}