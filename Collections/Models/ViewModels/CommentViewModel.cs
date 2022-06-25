namespace Collections.Models.ViewModels
{
    public class CommentViewModel
    {
        public string Body { get; set; }

        public CommentUserViewModel User { get; set; }

        public string CreatedAt { get; set; }
    }

    public class CommentUserViewModel
    {
        public string Name { get; set; }

        public string Image { get; set; }
    }
}
