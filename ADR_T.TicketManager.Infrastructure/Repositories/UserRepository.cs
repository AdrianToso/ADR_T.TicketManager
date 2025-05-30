using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence;

namespace ADR_T.TicketManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
        }
        public async Task DeleteAsync(User entity)
        {
            _context.Users.Remove(entity);
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<IReadOnlyList<User>> ListAllAsync()
        {
            return await _context.Users
                .ToListAsync();
        }
        public async Task UpdateAsync(User entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
        public async Task<List<User>> GetByIdsAsync(IEnumerable<Guid> userIds)
        {
           return await _context.Users
                        .Where(u => userIds.Contains(u.Id)) 
                       .ToListAsync();
        }
    }
}