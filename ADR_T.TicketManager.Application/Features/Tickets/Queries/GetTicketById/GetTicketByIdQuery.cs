using MediatR;
using ADR_T.TicketManager.Application.DTOs;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
public record GetTicketByIdQuery(Guid TicketId) : IRequest<TicketDto>;
