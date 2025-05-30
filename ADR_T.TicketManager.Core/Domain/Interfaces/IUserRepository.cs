using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IUserRepository : IRepository<User>
{
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> userIds);
}