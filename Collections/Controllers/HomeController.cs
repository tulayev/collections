using Collections.Models;
using Collections.Models.ViewModels;
using Collections.Services.Admin.Collections;
using Collections.Services.Admin.Items;
using Collections.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collections.Controllers
{
    public class HomeController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ICollectionService _collectionService;

        public HomeController(IItemService itemService, ICollectionService collectionService)
        {
            _itemService = itemService;
            _collectionService = collectionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tag, int? collection, string user, int? page)
        {
            const int perPage = 12;
            IQueryable<Item> items;

            if (!string.IsNullOrEmpty(tag))
            {
                items = _itemService.GetItemsByTag(tag);
            }
            else if (!string.IsNullOrEmpty(user) && collection.HasValue)
            {
                items = _itemService.GetItemsByCollection(collection.Value, user);
            }
            else
            {
                items = _itemService.GetLatestItems();
            }

            var paginatedItems = await PaginatedList<Item>.CreateAsync(items.AsNoTracking(), page ?? 1, perPage);
            var collections = await _collectionService.GetTopCollectionsAsync();

            return View(new HomePageViewModel
            {
                Items = paginatedItems,
                Collections = collections
            });
        }

        [HttpGet]
        public async Task<IActionResult> Show(string slug)
        {
            var item = await _itemService.GetItemBySlugAsync(slug);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }
    }
}