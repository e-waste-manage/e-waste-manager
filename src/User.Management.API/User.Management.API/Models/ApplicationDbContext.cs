using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace User.Management.API.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("dbo");
            builder.Entity<ApplicationUser>(
                entity =>
                {
                    entity.ToTable(name: "User");
                }
            );

            builder.Entity <IdentityRole>(
                entity =>
                {
                    entity.ToTable(name: "Role");
                }
            );
            builder.Entity<IdentityUserRole<string>>(
                entity =>
                {
                    entity.ToTable(name: "UserRoles");
                }
            );
            builder.Entity<IdentityUserClaim<string>>(
                entity =>
                {
                    entity.ToTable(name: "UserClaims");
                }
            );
            builder.Entity<IdentityUserLogin<string>>(
                entity =>
                {
                    entity.ToTable(name: "UserLogins");
                }
            );
            builder.Entity<IdentityRoleClaim<string>>(
                entity =>
                {
                    entity.ToTable(name: "RoleClaims");
                }
            ); 
            builder.Entity<IdentityUserToken<string>>(
                entity =>
                {
                    entity.ToTable(name: "UserTokens");
                }
            );
            SeedRoles(builder);


        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "Doner", ConcurrencyStamp = "2", NormalizedName = "Doner" },
                 new IdentityRole() { Name = "Receiver", ConcurrencyStamp = "3", NormalizedName = "Receiver" }

                );
        }
    }
}
