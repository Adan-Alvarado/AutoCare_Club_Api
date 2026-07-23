using AutoCare_Club.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club.Api.Database
{
    public class AutoCareDbContext : DbContext
    {
        public AutoCareDbContext(
            DbContextOptions<AutoCareDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceEntity> Services { get; set; }
    }
}