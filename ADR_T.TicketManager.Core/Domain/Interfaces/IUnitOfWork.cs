using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITicketRepository Tickets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
