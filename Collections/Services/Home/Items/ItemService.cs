using Collections.Models;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.Items
{
    public partial class ItemService
    {
        public IQueryable<Item> GetLatestItems()
        {
            return _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .Include(i => i.Collection)
                    .ThenInclude(c => c.User)
                .OrderByDescending(i => i.CreatedAt);
        }

        public IQueryable<Item> GetItemsByTag(string tag)
        {
            return _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .Include(i => i.Collection)
                    .ThenInclude(c => c.User)
                .Where(i => i.Tags.Any(t => t.Name.Contains(tag)))
                .OrderByDescending(i => i.CreatedAt);
        }

        public IQueryable<Item> GetItemsByCollection(int collectionId, string username)
        {
            return _db.Items
                .Include(i => i.Tags)
                .Include(i => i.File)
                .Include(i => i.Fields)
                .Include(i => i.Collection)
                    .ThenInclude(c => c.User)
                .Where(i => i.Collection.Id == collectionId && i.Collection.User.Name == username)
                .OrderByDescending(i => i.CreatedAt);
        }

        public async Task<Item> GetItemBySlugAsync(string slug)
        {
            return await _db.Items
                .Include(i => i.Fields)
                .Include(i => i.File)
                .Include(i => i.Collection)
                .FirstOrDefaultAsync(i => i.Slug == slug);
        }
    }
}
