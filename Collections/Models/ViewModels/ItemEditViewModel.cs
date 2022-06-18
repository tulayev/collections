namespace Collections.Models.ViewModels
{
    public class ItemEditViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Tags { get; set; }

        public List<Field> Fields { get; set; }

#nullable enable
        public string? ExistingImage { get; set; }

        public IFormFile? Image { get; set; }
#nullable disable
    }
}
