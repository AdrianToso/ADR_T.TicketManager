using ADR_T.NotificationService.Domain.Entities;

namespace ADR_T.NotificationService.Application.Persistence;
public interface INotificationUnitOfWork
{
    Task SaveAsync(NotificationLog notification, CancellationToken cancellationToken = default);
}