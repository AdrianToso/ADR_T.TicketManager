using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Core.Domain.Entities;
public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    protected EntityBase(Guid id)
    {
        Id = id;
    }
    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }
    public DateTime FechacCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechacActualizacion { get; set; }
    public bool IsDeleted { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }
    public void ClearDomainEvent()
    {
        _domainEvents.Clear();
    }
}
