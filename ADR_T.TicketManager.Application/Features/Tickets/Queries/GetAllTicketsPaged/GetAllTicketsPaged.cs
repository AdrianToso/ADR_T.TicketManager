using MediatR;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTicketsPaged;
public class GetAllTicketsPagedQuery : IRequest<PagedResponse<List<TicketDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public TicketStatus? StatusFilter { get; set; }
}