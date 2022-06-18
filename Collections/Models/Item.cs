using System.ComponentModel.DataAnnotations;

namespace Collections.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CollectionId  { get; set; }

#nullable enable
        public string? Image { get; set; }
#nullable disable

        public DateTime CreatedAt { get; set; }

        public AppCollection Collection { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Field> Fields { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
