using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Elastic;
using Collections.Services.Image;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SlugGenerator;
using System.Transactions;

namespace Collections.Services.Admin.Items
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;
        private readonly IElasticClientService _elasticClientService;

        public ItemService(
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

        public async Task<PaginatedList<Item>> GetUserItemsAsync(string userId, string sort, string filter, string search, int? page)
        {
            var user = string.IsNullOrEmpty(userId)
                ? await _userManager.FindByEmailAsync(userId)
                : await _userManager.FindByIdAsync(userId);

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }

            var userCollections = await _db.Collections
                .Where(c => c.UserId == user.Id)
                .Select(c => c.Id)
                .ToListAsync();

            var userItems = _db.Items
                .Include(i => i.Collection)
                .Include(i => i.File)
                .Where(i => userCollections.Contains(i.CollectionId));

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                userItems = userItems.Where(i => i.Name.ToLower().Contains(lowerSearch) ||
                                                 i.Collection.Name.ToLower().Contains(lowerSearch));
            }

            userItems = sort switch
            {
                "name_desc" => userItems.OrderByDescending(i => i.Name),
                "collection_desc" => userItems.OrderByDescending(i => i.Collection.Name),
                _ => userItems.OrderBy(i => i.Name),
            };

            return await PaginatedList<Item>.CreateAsync(userItems.AsNoTracking(), page ?? 1, 10);
        }

        public async Task<bool> CanAccessCollectionAsync(string userEmail, int collectionId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var collection = await _db.Collections.FindAsync(collectionId);

            if (collection == null)
            {
                return false;
            }
            if (collection.UserId == user.Id)
            {
                return true;
            }

            var admins = await _userManager.GetUsersInRoleAsync(Constants.Roles.RoleAdmin);
            return admins.Any(a => a.Id == user.Id);
        }

        public async Task<bool> CreateItemAsync(ItemCreateViewModel model, string[] keys, string[] values, int[] types, string userEmail)
        {
            if (keys != null && values != null && keys.Length != values.Length)
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            var file = model.Image != null ? await UploadFileAsync(null, model.Image) : null;

            var item = new Item
            {
                Name = model.Name,
                Slug = string.Empty,
                CollectionId = model.CollectionId,
                Tags = GetTags(model.Tags),
                Fields = GetCustomFields((0, keys, values, types)),
                FileId = file?.Id,
                CreatedAt = DateTime.UtcNow
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
                    Image = file?.Url
                },
                Comments = new List<CommentDto>()
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ItemEditViewModel> GetItemForEditAsync(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return null;
            }

            return new ItemEditViewModel
            {
                Id = item.Id,
                Name = item.Name,
                ExistingImage = item.File?.Url,
                Tags = string.Join(",", item.Tags.Select(t => t.Name)),
                Fields = item.Fields
            };
        }

        public async Task<bool> EditItemAsync(ItemEditViewModel model, string[] keys, string[] values, int[] types)
        {
            if (keys != null && values != null && keys.Length != values.Length)
            {
                return false;
            }

            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            var file = model.Image != null 
                ? await UploadFileAsync(item.File, model.Image) 
                : item.File;

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

            return true;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return false;
            }

            if (item.File != null)
            {
                await _imageService.DeleteImageAsync(item.File.PublicId);
                _db.Files.Remove(item.File);
            }

            _db.Tags.RemoveRange(item.Tags);
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();

            await _elasticClientService.RemoveFromElasticIndexAsync<Item>(id);

            return true;
        }

        private async Task<AppFile> UploadFileAsync(AppFile file, IFormFile formFile)
        {
            if (file != null)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await _imageService.DeleteImageAsync(file.PublicId);

                var uploadResult = await _imageService.UploadImageAsync(formFile);
                file.PublicId = uploadResult.PublicId;
                file.Url = uploadResult.Url.AbsoluteUri;

                ts.Complete();
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

            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => new Tag { Name = t.Trim().ToLower() }).ToList();
        }

        private List<Field> GetCustomFields((int ItemId, string[] Keys, string[] Values, int[] Types) data, List<Field> existingFields = null)
        {
            if (existingFields != null)
            {
                _db.Fields.RemoveRange(existingFields);
                existingFields.Clear();
            }

            var fields = new List<Field>();

            for (var i = 0; i < data.Keys.Length; i++)
            {
                fields.Add(new Field
                {
                    ItemId = data.ItemId,
                    Key = data.Keys[i],
                    Value = data.Values[i],
                    Type = (FieldType)data.Types[i]
                });
            }

            if (data.ItemId == 0)
            {
                _db.Fields.AddRange(fields);
            }

            return fields;
        }
    }
}
