using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
public record AssignTicketCommand(Guid TicketId, Guid TecnicoId, Guid AsignadorUserId) : IRequest<Unit>;
