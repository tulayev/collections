using Collections.Models.ViewModels;
using Collections.Services.Admin.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userId, string sort, string filter, string search, int? page)
        {
            var items = await _itemService.GetUserItemsAsync(userId ?? User.Identity.Name, sort, filter, search, page);

            ViewData["CurrentSort"] = sort;
            ViewData["CurrentFilter"] = search;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sort) ? "name_desc" : "";
            ViewData["CollectionSortParam"] = string.IsNullOrEmpty(sort) ? "collection_desc" : "";

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? collection)
        {
            if (collection == null || !await _itemService.CanAccessCollectionAsync(User.Identity.Name, collection.Value))
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ItemCreateViewModel model, 
            string[] keys, 
            string[] values, 
            int[] types)
        {
            var success = await _itemService.CreateItemAsync(model, keys, values, types, User.Identity.Name);
            return success ? RedirectToAction("Index") : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _itemService.GetItemForEditAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemEditViewModel model, string[] keys, string[] values, int[] types)
        {
            var success = await _itemService.EditItemAsync(model, keys, values, types);
            return success ? RedirectToAction("Index") : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _itemService.DeleteItemAsync(id);
            return success ? RedirectToAction("Index") : NotFound();
        }
    }
}