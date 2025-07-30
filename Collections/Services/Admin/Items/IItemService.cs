using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Utils;

namespace Collections.Services.Admin.Items
{
    public interface IItemService
    {
        Task<PaginatedList<Item>> GetUserItemsAsync(string userId, string sort, string filter, string search, int? page);
        Task<bool> CanAccessCollectionAsync(string userEmail, int collectionId);
        Task<bool> CreateItemAsync(ItemCreateViewModel model, string[] keys, string[] values, int[] types, string userEmail);
        Task<ItemEditViewModel> GetItemForEditAsync(int id);
        Task<bool> EditItemAsync(ItemEditViewModel model, string[] keys, string[] values, int[] types);
        Task<bool> DeleteItemAsync(int id);
    }
}
