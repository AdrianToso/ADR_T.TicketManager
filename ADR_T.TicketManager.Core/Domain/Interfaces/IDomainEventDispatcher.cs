using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchDomainEventsAsync(IEnumerable<EntityBase> entitiesWithEvents);
}