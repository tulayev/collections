namespace Collections.Models.ViewModels
{
    public class ItemCreateViewModel
    {
        public string Name { get; set; }
        public string Tags { get; set; }
        public int CollectionId { get; set; }
        public IFormFile Image { get; set; }
    }
}
