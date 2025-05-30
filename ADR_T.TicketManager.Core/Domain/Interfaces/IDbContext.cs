namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
}
