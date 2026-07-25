using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club.Api.Database
{
    public class AutoCareDbContext
        : IdentityDbContext<UserEntity, RoleEntity, string>
    {
        public AutoCareDbContext(
            DbContextOptions<AutoCareDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserEntity>().ToTable("users");
            builder.Entity<RoleEntity>().ToTable("roles");
            builder.Entity<IdentityUserRole<string>>()
                .ToTable("users_roles");
            builder.Entity<IdentityUserClaim<string>>()
                .ToTable("users_claims");
            builder.Entity<IdentityRoleClaim<string>>()
                .ToTable("roles_claims");
            builder.Entity<IdentityUserLogin<string>>()
                .ToTable("users_logins");
            builder.Entity<IdentityUserToken<string>>()
                .ToTable("users_tokens");
        }

        public DbSet<ServiceEntity> Services { get; set; }
        public DbSet<VehicleEntity> Vehicles { get; set; }
    }
}
