namespace Collections.Models
{
    public class FieldGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public FieldType FieldType { get; set; }
    }

    public enum FieldType 
    {
        Number = 1,
        Text = 2,
        Textarea = 3,
        Boolean = 4
    }
}
