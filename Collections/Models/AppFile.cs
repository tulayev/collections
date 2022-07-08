namespace Collections.Models
{
    public class AppFile
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

#nullable enable
        public string? S3Key { get; set; }

        public string? S3Path { get; set; }
    }
}
