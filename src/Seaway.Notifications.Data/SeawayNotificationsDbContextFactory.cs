using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Seaway.Notifications.Data;

internal sealed class SeawayNotificationsDbContextFactory
    : IDesignTimeDbContextFactory<SeawayNotificationsDbContext>
{
    public SeawayNotificationsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SeawayNotificationsDbContext>()
            .UseSqlServer(
                "Server=SPC-DB01;Database=SeawayNotifications;Integrated Security=True;TrustServerCertificate=True;",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"))
            .Options;

        return new SeawayNotificationsDbContext(options);
    }
}

