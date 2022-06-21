namespace Collections.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public int ItemId { get; set; }
        
        public Item Item { get; set; }

#nullable enable
        public DateTime? CreatedAt { get; set; }
#nullable disable
    }
}
