using System.ComponentModel.DataAnnotations;

namespace Collections.Models
{
    public class AppCollection
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public List<Item> Items { get; set; }
    }
}
