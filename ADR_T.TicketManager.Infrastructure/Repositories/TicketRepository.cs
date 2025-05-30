using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;
        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Tickets.FindAsync(id);
        }
        public async Task<IReadOnlyList<Ticket>> ListAllAsync()
        {
            return await _context.Tickets.ToListAsync();
        }
      
        public async Task AddAsync(Ticket entity)
        {
            await _context.Tickets.AddAsync(entity);
        }

        public async Task UpdateAsync(Ticket entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
        public async Task DeleteAsync(Ticket entity)
        {
            _context.Tickets.Remove(entity);
        }
        public async Task<List<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(Guid userId)
        {
            return await _context.Tickets
                .Where(t => t.CreadoByUserId == userId || t.AsignadoUserId == userId)
                .ToListAsync();
        }

        public async Task<(List<Ticket> data, int totalRecords)> GetPagedTicketsAsync(
                int pageNumber,
                int pageSize,
                TicketStatus? statusFilter = null)
        {
            var query = _context.Tickets.AsQueryable();

            if (statusFilter.HasValue)
                query = query.Where(t => t.Status == statusFilter.Value);

            int totalRecords = await query.CountAsync();
            List<Ticket> data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
