using MediatR;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence;


namespace ADR_T.TicketManager.Infrastructure.Services;
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchDomainEventsAsync(AppDbContext context) 
    {
        if (context == null) 
            throw new ArgumentNullException(nameof(context), "Error de contexto: el contexto no puede ser nulo.");

        var entities = context.ChangeTracker
             .Entries<EntityBase>()
             .Where(e => e.Entity.DomainEvents.Any())
             .Select(e => e.Entity)
             .ToList(); 

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvent());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent); 
        }
    }

    async Task IDomainEventDispatcher.DispatchDomainEventsAsync(IDbContext dbContext)
    {
        if (dbContext is AppDbContext appDbContext)
        {
            await DispatchDomainEventsAsync(appDbContext);
        }
        else
        {
            throw new InvalidOperationException("El contexto proporcionado no es una instancia de AppDbContext.");
        }
    }
}