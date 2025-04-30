using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collections.Models.Configurations
{
    public class AppFileConfig : IEntityTypeConfiguration<AppFile>
    {
        public void Configure(EntityTypeBuilder<AppFile> builder)
        {
            builder.Property(p => p.PublicId).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Url).HasMaxLength(255).IsRequired();
        }
    }
}
