using Collections.Models;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Collections.Data.Seeders
{
    public static class InitialSeeder
    {
        public static void SeedAdmin(ModelBuilder modelBuilder)
        {
            string adminRoleId = Guid.NewGuid().ToString();
            string userRoleId = Guid.NewGuid().ToString();
            string superAdminId = Guid.NewGuid().ToString();
            string adminLogin = "admin@collections.com";
            string adminPassword = "admin.1";

            modelBuilder.Entity<IdentityRole>().HasData(new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = Roles.RoleAdmin,
                    NormalizedName = Roles.RoleAdmin.ToUpper(),
                    ConcurrencyStamp = adminRoleId
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = Roles.RoleUser,
                    NormalizedName = Roles.RoleUser.ToUpper(),
                    ConcurrencyStamp = userRoleId
                },
            });

            var superAdmin = new User
            {
                Id = superAdminId,
                Name = "Admin",
                Email = adminLogin,
                EmailConfirmed = false,
                NormalizedEmail = adminLogin.ToUpper(),
                UserName = adminLogin,
                NormalizedUserName = adminLogin.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString().ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };

            var hasher = new PasswordHasher<User>();
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, adminPassword);

            modelBuilder.Entity<User>().HasData(superAdmin);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = superAdminId
            });
        }
    }
}
