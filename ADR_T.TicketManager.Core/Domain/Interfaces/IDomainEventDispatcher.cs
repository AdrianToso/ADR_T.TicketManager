namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IDomainEventDispatcher
{
    Task DispatchDomainEventsAsync(IDbContext context);
}
