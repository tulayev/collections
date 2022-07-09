using Collections.Utils;

namespace Collections.Models.ViewModels
{
    public class HomePageViewModel
    {
        public PaginatedList<Item> Items { get; set; }

        public List<AppCollectionViewModel> Collections { get; set; }
    }
}
