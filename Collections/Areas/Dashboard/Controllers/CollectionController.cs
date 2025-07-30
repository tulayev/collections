using Collections.Models;
using Collections.Services.Admin.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ICollectionService _collectionService;
        private readonly IWebHostEnvironment _env;

        public CollectionController(ICollectionService collectionService, IWebHostEnvironment env)
        {
            _collectionService = collectionService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userId, string sort, string filter, string search, int? page)
        {
            var collections = await _collectionService.GetUserCollectionsAsync(userId ?? User.Identity.Name, sort, filter, search, page);

            ViewData["CurrentSort"] = sort;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sort) ? "name_desc" : "";
            ViewData["CurrentFilter"] = search;

            return View(collections);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppCollection model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _collectionService.CreateCollectionAsync(model, User.Identity.Name);
            return success ? RedirectToAction("Index") : View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var collection = await _collectionService.GetCollectionAsync(id);
            return collection == null ? NotFound() : View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppCollection model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _collectionService.UpdateCollectionAsync(model);
            return success ? RedirectToAction("Index") : View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _collectionService.DeleteCollectionAsync(id);
            return success ? RedirectToAction("Index") : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Export(string userId)
        {
            var (content, filename) = await _collectionService.ExportCollectionsAsync(userId, _env.WebRootPath);

            if (content == null)
            {
                return RedirectToAction("Index");
            }

            return File(content, "application/octet-stream", filename);
        }
    }
}
