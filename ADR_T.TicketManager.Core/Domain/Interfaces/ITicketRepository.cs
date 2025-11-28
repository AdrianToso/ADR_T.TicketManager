using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface ITicketRepository : IRepository<Ticket>
{
    Task<(List<Ticket> data, int totalRecords)> GetPagedTicketsAsync(
         int pageNumber,
         int pageSize,
         TicketStatus? statusFilter = null,
         CancellationToken cancellationToken = default);
}

