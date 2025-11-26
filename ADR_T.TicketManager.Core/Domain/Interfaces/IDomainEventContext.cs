using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;

public interface IDomainEventContext
{
    IEnumerable<EntityBase> GetEntitiesWithEvents();
}