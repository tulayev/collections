using Collections.Data;
using Microsoft.EntityFrameworkCore;

namespace Collections.Services.Admin.FieldGroup
{
    public class FieldGroupService : IFieldGroupService
    {
        private readonly ApplicationDbContext _db;

        public FieldGroupService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Models.FieldGroup>> GetAllFieldGroupsAsync()
        {
            return await _db.FieldGroups.ToListAsync();
        }
    }
}
