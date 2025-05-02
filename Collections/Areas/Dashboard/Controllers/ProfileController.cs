using Collections.Models.ViewModels;
using Collections.Services.Admin.ProfileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var vm = await _profileService.GetProfileAsync(User.Identity.Name);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _profileService.UpdateProfileAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Не удалось сохранить профиль");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
