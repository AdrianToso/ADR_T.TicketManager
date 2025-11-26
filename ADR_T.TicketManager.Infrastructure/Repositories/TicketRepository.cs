// ADR_T.TicketManager.Infrastructure\Repositories\TicketRepository.cs

using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    private readonly AppDbContext _dbContext;

    public TicketRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<Ticket> data, int totalRecords)> GetPagedTicketsAsync(
        int pageNumber,
        int pageSize,
        TicketStatus? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var totalRecords = await _dbContext.Tickets.CountAsync(cancellationToken);

        var data = await _dbContext.Tickets
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (data, totalRecords);
    }
}