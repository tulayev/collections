using Collections.Data.Seeders;
using Collections.Models;
using Collections.Services.Image;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Collections.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<AppCollection> Collections { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<FieldGroup> FieldGroups { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AppFile> Files { get; set; }
        private readonly IImageService _imageService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IImageService imageService) : base(options)
        {
            _imageService = imageService;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.ChangeTracker.DetectChanges();

            var entities = this.ChangeTracker.Entries()
                        .Where(t => t.State == EntityState.Deleted)
                        .Select(t => t.Entity)
                        .ToArray();

            foreach (var entity in entities)
            {
                if (entity is AppFile appFile)
                {
                    var file = entity as AppFile;

                    if (!string.IsNullOrWhiteSpace(file.PublicId))
                    {
                        await _imageService.DeleteImageAsync(file.PublicId);
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            InitialSeeder.SeedAdmin(modelBuilder);
            InitialSeeder.SeedFieldGrups(modelBuilder);
        }
    }
}
