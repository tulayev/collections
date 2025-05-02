using Collections.Data.Repositories;
using Collections.Models;

namespace Collections.Services.Admin.Tags
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            var tags = await _unitOfWork.Repository<Tag>().GetAllAsync();
            return tags.ToList();
        }
    }
}
