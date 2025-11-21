using ADR_T.TicketManager.Infrastructure.Repositories;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Events;
using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Exceptions;

namespace ADR_T.TicketManager.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;
    private readonly ILogger<UnitOfWork> _logger;
    public ITicketRepository TicketRepository { get; }
    public IUserRepository UserRepository { get; }
    public UnitOfWork(AppDbContext context, IMediator mediator, ILogger<UnitOfWork> logger, IEventBus eventBus)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        TicketRepository = new TicketRepository(_context);
        UserRepository = new UserRepository(_context);
        TicketRepository = new TicketRepository(_context);
        UserRepository = new UserRepository(_context);
    }
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                _logger.LogInformation("Cambios guardados en la base de datos ({Result} entidades afectadas). Despachando eventos...", result);
                await DispatchEventsToBusAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("No se detectaron cambios en la base de datos. No se despacharán eventos.");
            }

            return result;
        }
        catch (DbUpdateException dbex)
        {
            _logger.LogError(dbex, "Error de base de datos durante CommitAsync.");
            throw new PersistenceException("Ocurrió un error de persistencia al guardar los cambios.", dbex);
        }
      
    }
    private async Task DispatchEventsToBusAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<EntityBase>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        if (!entitiesWithEvents.Any())
        {
            _logger.LogInformation("No se encontraron eventos de dominio para despachar.");
            return;
        }
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvent());

        _logger.LogInformation("Publicando {DomainEventCount} eventos de dominio para publicar en el bus...", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogDebug("Intentando publicar evento: {EventType} - ID: {EventId}",
                                 domainEvent.GetType().FullName,
                                 GetEventId(domainEvent));

                await _eventBus.PublishAsync(domainEvent, cancellationToken);

                _logger.LogInformation("Evento {EventType} - ID: {EventId} publicado exitosamente.",
                                    domainEvent.GetType().FullName,
                                    GetEventId(domainEvent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar el evento {EventType} - ID: {EventId} al bus. Evento: {@DomainEvent}",
                                 domainEvent.GetType().FullName,
                                 GetEventId(domainEvent),
                                 domainEvent);
            }
        }
    }
    // helper para obtener un ID identificable del evento
    private string GetEventId(IDomainEvent domainEvent)
    {
        if (domainEvent is TicketAsignadoEvent tae) return $"TicketId:{tae.TicketId}";
        if (domainEvent is TicketActualizadoEvent tue) return $"TicketId:{tue.TicketId}";
        if (domainEvent is TicketCreadoEvent tce) return $"TicketId:{tce.TicketId}";
        return "N/A";
    }
}