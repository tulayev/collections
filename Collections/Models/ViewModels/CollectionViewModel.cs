using System.ComponentModel.DataAnnotations;

namespace Collections.Models.ViewModels
{
    public class CollectionViewModel
    {
        [Required]
        public string Name { get; set; }
        public string UserId { get; set; }
        public int FieldNumber { get; set; }
        public int FieldText { get; set; }
        public int FieldTextarea { get; set; }
        public int FieldCheck { get; set; }
        public int FieldDate { get; set; }
    }
}
