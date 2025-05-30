using System.Text.Json.Serialization;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Core.Domain.Events;
public sealed class TicketAsignadoEvent : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid TicketId { get; }
    public string Titulo { get; }
    public Guid AsignadoUserId { get; }
    public string? AsignadoUserMail { get; }
    public Guid? AsignadorUserId { get; }

    // Constructor usado en tiempo de ejecución al disparar el evento
    public TicketAsignadoEvent(Ticket ticket, User assignedUser, Guid? asignadorUserId = null)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));

        TicketId = ticket.Id;
        Titulo = ticket.Titulo;
        AsignadoUserId = assignedUser.Id;
        AsignadoUserMail = assignedUser.Email;
        AsignadorUserId = asignadorUserId;
        OccurredOn = DateTime.UtcNow;
    }

    // Constructor para DESERIALIZACIÓN JSON
    [JsonConstructor]
    private TicketAsignadoEvent(
        DateTime occurredOn,
        Guid ticketId,
        string titulo,
        Guid asignadoUserId,
        string? asignadoUserMail,
        Guid? asignadorUserId)
    {
        OccurredOn = occurredOn;
        TicketId = ticketId;
        Titulo = titulo;
        AsignadoUserId = asignadoUserId;
        AsignadoUserMail = asignadoUserMail;
        AsignadorUserId = asignadorUserId;
    }
}