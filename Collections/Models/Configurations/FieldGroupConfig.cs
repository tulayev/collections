using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collections.Models.Configurations
{
    public class FieldGroupConfig : IEntityTypeConfiguration<FieldGroup>
    {
        public void Configure(EntityTypeBuilder<FieldGroup> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        }
    }
}
