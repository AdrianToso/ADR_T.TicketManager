using MediatR;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;

public record CreateTicketCommand(
    string Titulo, 
    string Descripcion, 
    TicketPriority Prioridad, 
    Guid CreadoByUserId
    ) : IRequest<Guid>;
                   

