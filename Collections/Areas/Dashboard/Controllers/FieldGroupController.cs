using Collections.Models;
using Collections.Services.Admin.FieldGroup;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/fieldgroups")]
    public class FieldGroupController : ControllerBase
    {
        private readonly IFieldGroupService _fieldGroupService;

        public FieldGroupController(IFieldGroupService fieldGroupService)
        {
            _fieldGroupService = fieldGroupService;
        }

        [HttpGet]
        public async Task<List<FieldGroup>> Get()
        {
            return await _fieldGroupService.GetAllFieldGroupsAsync();
        }
    }
}
