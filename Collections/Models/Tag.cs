﻿namespace Collections.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Item> Items { get; set; }
    }
}
