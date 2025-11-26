using ADR_T.TicketManager.Application.DTOs;
using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
public record GetTicketByIdQuery(Guid TicketId) : IRequest<TicketDto>;
