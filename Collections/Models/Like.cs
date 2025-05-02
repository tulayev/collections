namespace Collections.Models
{
    public class Like
    {
        public int ItemId { get; set; }
        public string UserId { get; set; }
        public LikeType Type { get; set; }
        public Item Item { get; set; }
        public User User { get; set; }
    }

    public enum LikeType
    {
        Like = 1,
        Dislike = 0
    }
}
