using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collections.Models.Configurations
{
    public class ItemConfig : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Slug).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Image).HasMaxLength(255);
        }
    }
}
