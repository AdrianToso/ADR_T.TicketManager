using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ADR_T.TicketManager.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
                return new List<User>().AsReadOnly();

            var distinctIds = ids.Distinct().ToArray();

            var users = await _dbSet
                .Where(u => distinctIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            return users.AsReadOnly(); ;

        }
    }

}