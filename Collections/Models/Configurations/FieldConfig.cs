using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collections.Models.Configurations
{
    public class FieldConfig : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.Property(p => p.Key).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Value).HasMaxLength(255).IsRequired();
        }
    }
}
