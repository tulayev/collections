﻿using Collections.Data;
using Collections.Extensions;
using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Elastic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Areas.Dashboard.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IElasticClientService _elasticClientService;

        public CommentController(
            ApplicationDbContext db, 
            UserManager<User> userManager,
            IElasticClientService elasticClientService)
        {
            _db = db;
            _userManager = userManager;
            _elasticClientService = elasticClientService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int itemId) => Ok(new
        {
            comments = await _db.Comments
                .Include(c => c.User)
                    .ThenInclude(u => u.File)
                .Where(c => c.ItemId == itemId)
                .Select(c => new CommentViewModel
                {
                    Body = c.Body,
                    User = new CommentUserViewModel { Name = c.User.Name, Image = c.User.File.S3Path },
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync()
        });

        [HttpPost("post")]
        public async Task<IActionResult> Post(string username, int itemId, string body)
        {
            var user = await _userManager.FindByNameAsync(username);
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (user == null || item == null)
            {
                return Ok(new { error = "Error occured" });
            }

            var comment = new Comment
            {
                Body = body,
                UserId = user.Id,
                ItemId = item.Id,
                CreatedAt = DateTime.Now.SetKindUtc()
            };

            _db.Comments.Add(comment);
            
            await _db.SaveChangesAsync();

            await _elasticClientService.UpdateElasticItemAsync(itemId, new ElasticItemViewModel
            {
                Comments = await _db.Comments.Where(c => c.ItemId == itemId)
                                    .Select(c => new CommentDto { Body = c.Body })
                                    .ToListAsync()
            });

            return Ok(new { message = "ok" });
        }
    }
}
