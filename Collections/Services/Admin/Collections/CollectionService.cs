using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Collections.Services.Admin.Collections
{
    public partial class CollectionService : ICollectionService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public CollectionService(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<PaginatedList<AppCollection>> GetUserCollectionsAsync(string userId, string sort, string filter, string search, int? page)
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

            var collections = _db.Collections.Include(c => c.User)
                .Where(c => c.UserId == user.Id);

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                collections = collections.Where(c => c.Name.ToLower().Contains(lowerSearch));
            }

            collections = sort switch
            {
                "name_desc" => collections.OrderByDescending(c => c.Name),
                _ => collections.OrderBy(c => c.Name),
            };

            return await PaginatedList<AppCollection>.CreateAsync(collections.AsNoTracking(), page ?? 1, 10);
        }

        public async Task<bool> CreateCollectionAsync(AppCollection model, string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            model.UserId = user.Id;

            await _db.Collections.AddAsync(model);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AppCollection> GetCollectionAsync(int id)
        {
            return await _db.Collections.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateCollectionAsync(AppCollection model)
        {
            _db.Collections.Update(model);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCollectionAsync(int id)
        {
            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id);
            if (collection == null) return false;

            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] content, string filename)> ExportCollectionsAsync(string userId, string wwwRootPath)
        {
            var collections = await _db.Collections.Where(c => c.UserId == userId).ToListAsync();

            if (!collections.Any())
            {
                return (null, null);
            }

            var sb = new StringBuilder();
            sb.AppendLine("ID,Name");

            foreach (var c in collections)
            {
                sb.AppendLine($"{c.Id},{c.Name}");
            }

            var filename = "collections.csv";
            var path = Path.Combine(wwwRootPath, "uploads", filename);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, sb.ToString());
            return (File.ReadAllBytes(path), filename);
        }
    }
}
