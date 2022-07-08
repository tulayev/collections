using Collections.Data;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<User> _userManager;

        private readonly IFileHandler _fileHandler;
        
        private readonly IS3Handler _s3Handler;

        public ProfileController(ApplicationDbContext db, UserManager<User> userManager, IFileHandler fileHandler, IS3Handler s3Handler)
        {
            _db = db;
            _userManager = userManager;
            _fileHandler = fileHandler;
            _s3Handler = s3Handler;
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

            var user = await _userManager.Users
                .Include(u => u.File)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            var file = user.File;
            Claim imageClaim = null;

            if (model.Image != null)
            {
                if (file != null)
                {
                    string filename = await _fileHandler.UploadAsync(model.Image, file.Path);
                    string s3Key = await _s3Handler.UploadFileAsync(model.Image, file.S3Key);
                    file.Name = filename;
                    file.Path = _fileHandler.GeneratePath(filename);
                    file.S3Key = s3Key;
                    file.S3Path = await _s3Handler.GetPathAsync(s3Key);
                }
                else
                {
                    string filename = await _fileHandler.UploadAsync(model.Image);
                    string s3Key = await _s3Handler.UploadFileAsync(model.Image);
                    file = new AppFile
                    {
                        Name = filename,
                        Path = _fileHandler.GeneratePath(filename),
                        S3Key = s3Key,
                        S3Path = await _s3Handler.GetPathAsync(s3Key)
                    };
                    _db.Files.Add(file);
                }
                await _db.SaveChangesAsync();
                imageClaim = new Claim("Image", file.S3Path);
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
