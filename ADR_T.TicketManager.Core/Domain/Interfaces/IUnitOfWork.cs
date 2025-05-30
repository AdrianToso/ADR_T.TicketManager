namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    ITicketRepository TicketRepository { get; }
    IUserRepository UserRepository { get; }  
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
