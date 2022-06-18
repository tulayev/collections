namespace Collections.Models.ViewModels
{
    public class ItemCreateViewModel
    {
        public string Name { get; set; }

        public string Tags { get; set; }

        public int CollectionId { get; set; }

#nullable enable
        public IFormFile? Image { get; set; }
#nullable disable
    }
}
