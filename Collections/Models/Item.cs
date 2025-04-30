namespace Collections.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int CollectionId  { get; set; }
        public int? FileId { get; set; }
        public DateTime CreatedAt { get; set; }
        public AppCollection Collection { get; set; }
        public AppFile File { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Field> Fields { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
