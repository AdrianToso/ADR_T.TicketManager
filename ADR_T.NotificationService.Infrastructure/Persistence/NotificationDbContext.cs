using Microsoft.EntityFrameworkCore;
using ADR_T.NotificationService.Domain.Entities;

namespace ADR_T.NotificationService.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("NotificationLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.IsProcessed).IsRequired();
        });
    }
}
