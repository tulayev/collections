namespace Collections.Models.ViewModels
{
    public class ElasticItemViewModel
    {
        public int Id { get; set; }
        public ItemDto Item { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

    public class ItemDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public int CollectionId { get; set; }
    }

    public class CommentDto
    {
        public string Body { get; set; }
    }
}
