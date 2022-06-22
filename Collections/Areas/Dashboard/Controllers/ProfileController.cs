using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly IUploadHandler _uploadHandler;

        public ProfileController(UserManager<User> userManager, IUploadHandler uploadHandler)
        {
            _userManager = userManager;
            _uploadHandler = uploadHandler;
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            return View(new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            string image = String.Empty;
            Claim imageClaim = null;

            if (model.Image != null)
            {
                image = await _uploadHandler.UploadAsync(model.Image, user.Image);
                imageClaim = new Claim("Image", image);
                user.Image = image;
            }

            var hasher = new PasswordHasher<User>();

            user.Name = model.Name ?? user.Name;
            user.UserName = model.Email ?? user.UserName;
            user.Email = model.Email ?? user.Email;
            
            if (model.Password != null)
                user.PasswordHash = hasher.HashPassword(user, model.Password);

            var userClaims = await _userManager.GetClaimsAsync(user);

            if (imageClaim != null)
            {
                var oldImageClaim = userClaims.FirstOrDefault(c => c.Type == "Image");

                if (oldImageClaim != null)
                {
                    await _userManager.ReplaceClaimAsync(user, oldImageClaim, imageClaim);
                }
                else
                {
                    await _userManager.AddClaimAsync(user, imageClaim);
                }
            }

            await _userManager.ReplaceClaimAsync(user, userClaims.FirstOrDefault(c => c.Type == "Name"), new Claim("Name", user.Name));
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
