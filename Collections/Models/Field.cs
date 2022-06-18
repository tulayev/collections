namespace Collections.Models
{
    public class Field
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int FieldGroupId { get; set; }
        
        public FieldGroup FieldGroup { get; set; }
        
        public int ItemId { get; set; }
        
        public Item Item { get; set; }
    }
}
