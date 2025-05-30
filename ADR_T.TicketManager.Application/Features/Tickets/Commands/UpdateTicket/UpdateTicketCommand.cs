using MediatR;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
public class UpdateTicketCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Prioridad { get; set; }
    public Guid CreadoByUserId { get; set; }
}
