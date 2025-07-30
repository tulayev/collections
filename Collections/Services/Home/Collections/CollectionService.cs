using Collections.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.Collections
{
    public partial class CollectionService
    {
        public async Task<List<AppCollectionViewModel>> GetTopCollectionsAsync()
        {
            return await _db.Collections
                .Include(c => c.User)
                .Include(c => c.Items)
                .GroupBy(c => new { c.Id, c.Name, Author = c.User.Name, Total = c.Items.Count })
                .Select(c => new AppCollectionViewModel
                {
                    Id = c.Key.Id,
                    Name = c.Key.Name,
                    Author = c.Key.Author,
                    Total = c.Key.Total
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();
        }
    }
}
