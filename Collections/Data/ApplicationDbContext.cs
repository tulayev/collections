using Collections.Data.Seeders;
using Collections.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Collections.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            InitialSeeder.SeedAdmin(modelBuilder);
        }

        public DbSet<AppCollection> Collections { get; set; }

        public DbSet<Item> Items { get; set; }
        
        public DbSet<Tag> Tags { get; set; }

        public DbSet<FieldGroup> FieldGroups { get; set; }
        
        public DbSet<Field> Fields { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }
}
