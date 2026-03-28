using Microsoft.EntityFrameworkCore;
using Seaway.Notifications.Data.Entities;

namespace Seaway.Notifications.Data;

public class SeawayNotificationsDbContext : DbContext
{
    public SeawayNotificationsDbContext(DbContextOptions<SeawayNotificationsDbContext> options)
        : base(options) { }

    public DbSet<App> Apps => Set<App>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<App>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AppKey).IsUnique();
            e.Property(x => x.AppKey).HasMaxLength(50).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<NotificationType>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.AppId, x.TypeKey }).IsUnique();
            e.Property(x => x.TypeKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.App)
             .WithMany(x => x.NotificationTypes)
             .HasForeignKey(x => x.AppId);
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            e.Property(x => x.Recipient).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.NotificationType)
             .WithMany(x => x.Subscriptions)
             .HasForeignKey(x => x.NotificationTypeId);
        });

        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            e.Property(x => x.Recipient).HasMaxLength(500).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            e.Property(x => x.ErrorMessage).HasMaxLength(2000);
            e.HasOne(x => x.NotificationType)
             .WithMany()
             .HasForeignKey(x => x.NotificationTypeId);
        });
    }
}

