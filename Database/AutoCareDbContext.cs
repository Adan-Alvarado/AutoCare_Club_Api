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

            builder.Entity<VehicleEntity>()
                .HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(vehicle => vehicle.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderEntity>(entity =>
            {
                entity.Property(order => order.Total)
                    .HasPrecision(12, 2);

                entity.Property(order => order.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(order => order.AppointmentId)
                    .HasMaxLength(36);

                entity.Property(order => order.StripePaymentIntentId)
                    .HasMaxLength(100);

                entity.Property(order => order.PaymentStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("not_started");

                entity.HasIndex(order => order.UserId)
                    .IsUnique()
                    .HasFilter("\"Status\" = 'Draft'");

                entity.HasIndex(order => order.AppointmentId)
                    .IsUnique()
                    .HasFilter("\"AppointmentId\" IS NOT NULL");

                entity.HasIndex(order => order.StripePaymentIntentId)
                    .IsUnique()
                    .HasFilter("\"StripePaymentIntentId\" IS NOT NULL");

                entity.HasOne<UserEntity>()
                    .WithMany()
                    .HasForeignKey(order => order.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<VehicleEntity>()
                    .WithMany()
                    .HasForeignKey(order => order.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<AppointmentEntity>()
                    .WithMany()
                    .HasForeignKey(order => order.AppointmentId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderItemEntity>(entity =>
            {
                entity.Property(item => item.UnitPrice)
                    .HasPrecision(12, 2);

                entity.Property(item => item.Subtotal)
                    .HasPrecision(12, 2);

                entity.HasIndex(item => new
                {
                    item.OrderId,
                    item.ServiceId
                }).IsUnique();

                entity.HasOne(item => item.Order)
                    .WithMany(order => order.Items)
                    .HasForeignKey(item => item.OrderId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(item => item.Service)
                    .WithMany()
                    .HasForeignKey(item => item.ServiceId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppointmentEntity>(entity =>
            {

                entity.Property(appointment => appointment.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

                entity.Property(appointment => appointment.Notes)
                .HasMaxLength(500);

                entity.HasIndex(appointment => new
                {
                    appointment.AppointmentDate,
                    appointment.StartTime
                });

                entity.HasIndex(appointment => new
                {
                    appointment.UserId,
                    appointment.AppointmentDate
                });

                entity.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(appointment =>
                appointment.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<VehicleEntity>()
            .WithMany()
            .HasForeignKey(appointment =>
                appointment.VehicleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ServiceEntity>()
            .WithMany()
            .HasForeignKey(appointment =>
                appointment.ServiceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<TechnicianEntity>()
            .WithMany()
            .HasForeignKey(appointment =>
                appointment.TechnicianId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TechnicianEntity>(entity =>
            {
                entity.HasKey(technician =>
                    technician.UserId);

                entity.Property(technician =>
                    technician.Specialty)
                    .HasMaxLength(100);

                entity.HasOne(technician =>
                        technician.User)
                    .WithOne()
                    .HasForeignKey<TechnicianEntity>(
                        technician => technician.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public DbSet<ServiceEntity> Services { get; set; }
        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<ScheduleEntity> Schedules { get; set; }
        public DbSet<AppointmentEntity> Appointments { get; set; }
        public DbSet<TechnicianEntity> Technicians { get; set; }
    }
}
