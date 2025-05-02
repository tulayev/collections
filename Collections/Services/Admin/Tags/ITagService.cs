using Collections.Models;

namespace Collections.Services.Admin.Tags
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTagsAsync();
    }
}
