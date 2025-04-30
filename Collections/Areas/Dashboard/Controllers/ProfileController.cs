using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Transactions;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;

        public ProfileController(
            ApplicationDbContext db, 
            UserManager<User> userManager,
            IImageService imageService)
        {
            _db = db;
            _userManager = userManager;
            _imageService = imageService;
        }

        [HttpGet]
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

            var user = await _userManager.Users
                .Include(u => u.File)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            var file = user.File;

            Claim imageClaim = null;

            if (model.Image != null)
            {
                if (file != null)
                {
                    using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await _imageService.DeleteImageAsync(file.PublicId);

                        var uploadResult = await _imageService.UploadImageAsync(model.Image);

                        file.PublicId = uploadResult.PublicId;
                        file.Url = uploadResult.Url.AbsoluteUri;

                        ts.Complete();
                    }
                }
                else
                {
                    var uploadResult = await _imageService.UploadImageAsync(model.Image);

                    file = new AppFile
                    {
                        PublicId = uploadResult.PublicId,
                        Url = uploadResult.Url.AbsoluteUri,
                    };

                    _db.Files.Add(file);
                }

                await _db.SaveChangesAsync();

                imageClaim = new Claim("Image", file.Url);
            }

            var hasher = new PasswordHasher<User>();

            user.Name = model.Name ?? user.Name;
            user.UserName = model.Email ?? user.UserName;
            user.Email = model.Email ?? user.Email;
            user.FileId = file?.Id;
            
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
