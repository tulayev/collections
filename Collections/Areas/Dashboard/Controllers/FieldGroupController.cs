using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/fieldgroups")]
    public class FieldGroupController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public FieldGroupController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<FieldGroup>> Get() => await _db.FieldGroups.ToListAsync();
    }
}
