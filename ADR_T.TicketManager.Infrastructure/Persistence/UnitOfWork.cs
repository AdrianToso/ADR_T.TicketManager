using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IUserRepository _users;
    private readonly ITicketRepository _tickets;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        AppDbContext context,
        IDomainEventDispatcher domainEventDispatcher,
        IUserRepository users,
        ITicketRepository tickets,
        ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IUserRepository Users => _users;
    public ITicketRepository Tickets => _tickets;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // get entidades con eventos antes de guardar
            var entitiesWithEvents = _context.GetEntitiesWithEvents().ToList();
            // Guardar cambios en la base de datos primero
            var result = await _context.SaveChangesAsync(cancellationToken);
            // Despachar eventos después del guardado exitoso
            if (entitiesWithEvents.Any())
            {
                _logger.LogInformation("Despachando {Count} eventos de dominio", entitiesWithEvents.Count);
                await _domainEventDispatcher.DispatchDomainEventsAsync(entitiesWithEvents);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en UnitOfWork al guardar cambios");
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}