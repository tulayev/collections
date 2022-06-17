using System.ComponentModel.DataAnnotations;

namespace Collections.Models.ViewModels
{
    public class ItemEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Tags { get; set; }

#nullable enable
        public IFormFile? Image { get; set; }
#nullable disable
    }
}
