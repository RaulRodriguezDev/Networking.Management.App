using Microsoft.EntityFrameworkCore;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<DeviceLocation> DeviceLocations { get; set; }
        public DbSet<NetworkSetting> NetworkSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.UserId).HasColumnName("user_id");
                entity.Property(u => u.Name).HasColumnName("name").IsRequired();
                entity.Property(u => u.Username).HasColumnName("username").IsRequired();
                entity.Property(u => u.Password).HasColumnName("password").IsRequired();
                entity.Property(u => u.RoleId).HasColumnName("role_id").IsRequired();

            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("devices");
                entity.HasIndex(d => d.IpAddress).IsUnique();
                entity.HasKey(d => d.DeviceId);

                entity.Property(d => d.DeviceId).HasColumnName("device_id");
                entity.Property(d => d.DeviceName).HasColumnName("device_name");
                entity.Property(d => d.IpAddress).HasColumnName("ip_address").IsRequired();
                entity.Property(d => d.Status).HasColumnName("status").IsRequired();
                entity.Property(d => d.DeviceTypeId).HasColumnName("devicetype_id").IsRequired();
                entity.Property(d => d.DeviceLocationId).HasColumnName("location_id").IsRequired();
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("logs");
                entity.HasKey(l => l.LogId);

                entity.Property(l => l.LogId).HasColumnName("log_id");
                entity.Property(l => l.ActionType).HasColumnName("action_type").IsRequired();
                entity.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(l => l.Details).HasColumnName("details").IsRequired();
                entity.Property(l => l.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(l => l.DeviceId).HasColumnName("device_id");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasIndex(r => r.RoleName).IsUnique();
                entity.HasKey(r => r.RoleId);

                entity.Property(r => r.RoleId).HasColumnName("role_id");
                entity.Property(r => r.RoleName).HasColumnName("role_name").IsRequired();
                entity.Property(r => r.Description).HasColumnName("description");
            });

            modelBuilder.Entity<DeviceType>(entity =>
            {
                entity.ToTable("device_type");
                entity.HasIndex(dt => dt.TypeName).IsUnique();
                entity.HasKey(dt => dt.Id);

                entity.Property(dt => dt.Id).HasColumnName("devicetype_id");
                entity.Property(dt => dt.TypeName).HasColumnName("type_name").IsRequired();
                entity.Property(dt => dt.Description).HasColumnName("description");
            });

            modelBuilder.Entity<DeviceLocation>(entity =>
            {
                entity.ToTable("device_location");
                entity.HasIndex(dl => dl.LocationName).IsUnique();
                entity.HasKey(dl => dl.Id);

                entity.Property(dl => dl.Id).HasColumnName("location_id");
                entity.Property(dl => dl.LocationName).HasColumnName("location_name").IsRequired();
                entity.Property(dl => dl.LocationFloor).HasColumnName("location_floor");
            });

            modelBuilder.Entity<NetworkSetting>(entity =>
            {
                entity.ToTable("network_setting");
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Id).HasColumnName("network_setting_id");
                entity.Property(n => n.RangeStart).HasColumnName("range_start").IsRequired();
                entity.Property(n => n.RangeEnd).HasColumnName("range_end").IsRequired();
            });



            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.Device)
                .WithMany()
                .HasForeignKey(l => l.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceType)
                .WithMany(dt => dt.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceLocation)
                .WithMany(dl => dl.Devices)
                .HasForeignKey(d => d.DeviceLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

