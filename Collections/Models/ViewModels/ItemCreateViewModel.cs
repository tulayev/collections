using System.ComponentModel.DataAnnotations;

namespace Collections.Models.ViewModels
{
    public class ItemCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Tags { get; set; }

        public int CollectionId { get; set; }

#nullable enable
        public IFormFile? Image { get; set; }
#nullable disable
    }
}
