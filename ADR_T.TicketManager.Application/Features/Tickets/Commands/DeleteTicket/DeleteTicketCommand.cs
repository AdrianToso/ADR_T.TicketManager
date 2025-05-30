using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket
{
    public record DeleteTicketCommand(Guid TicketId) : IRequest<Unit>;
    
}
