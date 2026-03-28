using Microsoft.EntityFrameworkCore;

namespace Seaway.Notifications.Data;

public class SeawayNotificationsDbContext : DbContext
{
    public SeawayNotificationsDbContext(DbContextOptions<SeawayNotificationsDbContext> options)
        : base(options) { }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AppKey).HasMaxLength(50).IsRequired();
            e.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            e.Property(x => x.Recipient).HasMaxLength(500).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            e.Property(x => x.ErrorMessage).HasMaxLength(2000);
            e.Property(x => x.ChannelResponse).HasMaxLength(2000);
        });
    }
}
