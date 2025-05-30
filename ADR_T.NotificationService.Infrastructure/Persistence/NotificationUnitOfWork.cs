using ADR_T.NotificationService.Application.Persistence;
using ADR_T.NotificationService.Domain.Entities;

namespace ADR_T.NotificationService.Infrastructure.Persistence;
public class NotificationUnitOfWork : INotificationUnitOfWork
{
    private readonly NotificationDbContext _context;
    public NotificationUnitOfWork(NotificationDbContext context)
    {
        _context = context;
    }
    public async Task SaveAsync(NotificationLog notification, CancellationToken cancellationToken = default)
    {
        _context.NotificationLogs.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
