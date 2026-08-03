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
        public DbSet<Vlan> Vlans { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<StaticRoute> Routes { get; set; }
        public DbSet<AccessSetting> AccessSettings { get; set; }

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
                entity.Property(d => d.MacAddress).HasColumnName("mac_address").HasMaxLength(17);
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

            modelBuilder.Entity<AccessSetting>(entity =>
            {
                entity.ToTable("access_setting");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id).HasColumnName("access_setting_id");
                entity.Property(a => a.Mode).HasColumnName("mode").IsRequired();
                entity.Property(a => a.MasterIp).HasColumnName("master_ip");
            });



            modelBuilder.Entity<Vlan>(entity =>
            {
                entity.ToTable("vlans");
                entity.HasIndex(v => v.VlanNumber).IsUnique();
                entity.HasKey(v => v.VlanId);

                entity.Property(v => v.VlanId).HasColumnName("vlan_id");
                entity.Property(v => v.VlanNumber).HasColumnName("vlan_number").IsRequired();
                entity.Property(v => v.Name).HasColumnName("name").IsRequired();
                entity.Property(v => v.Description).HasColumnName("description");
            });

            modelBuilder.Entity<Port>(entity =>
            {
                entity.ToTable("ports");
                entity.HasKey(p => p.PortId);

                entity.Property(p => p.PortId).HasColumnName("port_id");
                entity.Property(p => p.Name).HasColumnName("name").IsRequired();
                entity.Property(p => p.IsEnabled).HasColumnName("is_enabled").IsRequired();
                entity.Property(p => p.IsUp).HasColumnName("is_up").IsRequired();
                entity.Property(p => p.DeviceId).HasColumnName("device_id").IsRequired();
                entity.Property(p => p.VlanId).HasColumnName("vlan_id");
            });

            modelBuilder.Entity<StaticRoute>(entity =>
            {
                entity.ToTable("routes");
                entity.HasKey(r => r.RouteId);

                entity.Property(r => r.RouteId).HasColumnName("route_id");
                entity.Property(r => r.Destination).HasColumnName("destination").IsRequired();
                entity.Property(r => r.Gateway).HasColumnName("gateway").IsRequired();
                entity.Property(r => r.Description).HasColumnName("description");
                entity.Property(r => r.DeviceId).HasColumnName("device_id").IsRequired();
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

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Vlan)
                .WithMany(v => v.Devices)
                .HasForeignKey(d => d.VlanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Port>()
                .HasOne(p => p.Device)
                .WithMany(d => d.Ports)
                .HasForeignKey(p => p.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Port>()
                .HasOne(p => p.Vlan)
                .WithMany()
                .HasForeignKey(p => p.VlanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StaticRoute>()
                .HasOne(r => r.Device)
                .WithMany(d => d.Routes)
                .HasForeignKey(r => r.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

