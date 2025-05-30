namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
