namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken=default) where T : class;
}
