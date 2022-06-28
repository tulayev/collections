using System.ComponentModel.DataAnnotations;

namespace Collections.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public int CollectionId  { get; set; }

#nullable enable
        public string? Image { get; set; }
        
        public DateTime? CreatedAt { get; set; }
#nullable disable


        public AppCollection Collection { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Field> Fields { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
