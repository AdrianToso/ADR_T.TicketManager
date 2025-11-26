using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IEventBus eventBus, ILogger<DomainEventDispatcher> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchDomainEventsAsync(IEnumerable<EntityBase> entitiesWithEvents)
    {
        if (entitiesWithEvents == null)
            throw new ArgumentNullException(nameof(entitiesWithEvents));

        var entitiesList = entitiesWithEvents.ToList();

        var domainEvents = entitiesList
            .SelectMany(e => e.DomainEvents)
            .ToList();

        _logger.LogInformation("Encontrados {EventCount} eventos de dominio en {EntityCount} entidades",
            domainEvents.Count, entitiesList.Count);

        // Limpiar eventos de cada entidad
        foreach (var entity in entitiesList)
        {
            entity.ClearDomainEvent();
        }

        // Publicar cada evento a través del EventBus
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogInformation("Publicando evento de dominio: {EventType}", domainEvent.GetType().Name);
                await _eventBus.PublishAsync(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar el evento de dominio {EventType}", domainEvent.GetType().Name);
            }
        }
    }
}