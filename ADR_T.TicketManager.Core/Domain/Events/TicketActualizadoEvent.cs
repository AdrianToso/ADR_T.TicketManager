using System.Text.Json.Serialization;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Core.Domain.Events;
public sealed class TicketActualizadoEvent : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid TicketId { get; }
    public string Titulo { get; }
    public TicketStatus StatusAnterior { get; }
    public TicketStatus StatusNuevo { get; }
    public TicketPriority PrioridadAnterior { get; }
    public TicketPriority PrioridadNueva { get; }
    public Guid ActualizadoPorUserId { get; }

    public TicketActualizadoEvent(
        Ticket ticket,
        TicketStatus statusAnterior,
        TicketPriority prioridadAnterior,
        Guid actualizadoPorUserId)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));

        TicketId = ticket.Id;
        Titulo = ticket.Titulo;
        StatusAnterior = statusAnterior;
        StatusNuevo = ticket.Status;
        PrioridadAnterior = prioridadAnterior;
        PrioridadNueva = ticket.Priority;
        ActualizadoPorUserId = actualizadoPorUserId;
        OccurredOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private TicketActualizadoEvent(DateTime occurredOn,
                                   Guid ticketId,
                                   string titulo,
                                   TicketStatus statusAnterior,
                                   TicketStatus statusNuevo,
                                   TicketPriority prioridadAnterior,
                                   TicketPriority prioridadNueva,
                                   Guid actualizadoPorUserId)
    {
        OccurredOn = occurredOn;
        TicketId = ticketId;
        Titulo = titulo;
        StatusAnterior = statusAnterior;
        StatusNuevo = statusNuevo;
        PrioridadAnterior = prioridadAnterior;
        PrioridadNueva = prioridadNueva;
        ActualizadoPorUserId = actualizadoPorUserId;
    }
}