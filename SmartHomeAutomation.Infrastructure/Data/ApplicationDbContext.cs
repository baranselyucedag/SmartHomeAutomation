using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;

namespace SmartHomeAutomation.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceLog> DeviceLogs { get; set; }
        public DbSet<AutomationRule> AutomationRules { get; set; }
        public DbSet<AutomationLog> AutomationLogs { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Scene> Scenes { get; set; }
        public DbSet<SceneDevice> SceneDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations - Make Room-User relationship optional
            modelBuilder.Entity<User>()
                .HasMany(u => u.Rooms)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .IsRequired(false) // Make UserId nullable
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Devices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Room configurations
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Devices)
                .WithOne(d => d.Room)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Device configurations
            modelBuilder.Entity<Device>()
                .HasMany(d => d.DeviceLogs)
                .WithOne(l => l.Device)
                .HasForeignKey(l => l.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Scene-Device many-to-many relationship
            modelBuilder.Entity<SceneDevice>()
                .HasOne(sd => sd.Scene)
                .WithMany(s => s.SceneDevices)
                .HasForeignKey(sd => sd.SceneId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SceneDevice>()
                .HasOne(sd => sd.Device)
                .WithMany(d => d.SceneDevices)
                .HasForeignKey(sd => sd.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            // AutomationRule configurations
            modelBuilder.Entity<AutomationRule>()
                .HasOne(ar => ar.Device)
                .WithMany()
                .HasForeignKey(ar => ar.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AutomationRule>()
                .HasOne(ar => ar.Scene)
                .WithMany()
                .HasForeignKey(ar => ar.SceneId)
                .OnDelete(DeleteBehavior.SetNull);

            // AutomationLog configurations
            modelBuilder.Entity<AutomationLog>()
                .HasOne(al => al.AutomationRule)
                .WithMany()
                .HasForeignKey(al => al.AutomationRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 