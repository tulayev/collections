using Collections.Models;
using Collections.Utils;

namespace Collections.Services.Admin.Collections
{
    public interface ICollectionService
    {
        Task<PaginatedList<AppCollection>> GetUserCollectionsAsync(string userId, string sort, string filter, string search, int? page);
        Task<bool> CreateCollectionAsync(AppCollection model, string userEmail);
        Task<AppCollection> GetCollectionAsync(int id);
        Task<bool> UpdateCollectionAsync(AppCollection model);
        Task<bool> DeleteCollectionAsync(int id);
        Task<(byte[] content, string filename)> ExportCollectionsAsync(string userId, string wwwRootPath);
    }
}
