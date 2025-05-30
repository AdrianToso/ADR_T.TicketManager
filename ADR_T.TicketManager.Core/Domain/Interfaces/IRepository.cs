using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IRepository<T> where T : EntityBase
{
    Task<T> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
