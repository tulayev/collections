using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collections.Models.Configurations
{
    public class AppCollectionConfig : IEntityTypeConfiguration<AppCollection>
    {
        public void Configure(EntityTypeBuilder<AppCollection> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        }
    }
}
