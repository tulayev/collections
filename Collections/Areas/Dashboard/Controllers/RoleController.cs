using Collections.Constants;
using Collections.Services.Admin.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = Roles.RoleAdmin)]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            var model = await _roleService.GetEditModelAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            var success = await _roleService.UpdateUserRolesAsync(userId, roles);

            if (!success)
            {
                return BadRequest();
            }

            return RedirectToAction("Index", "User", new { area = "Dashboard" });
        }
    }
}
