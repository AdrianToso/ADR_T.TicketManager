using System;
using System.Text.Json.Serialization;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Core.Domain.Events;
public sealed class TicketCreadoEvent : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid TicketId { get; }
    public string Titulo { get; }
    public Guid CreadoByUserId { get; }

    public TicketCreadoEvent(Ticket ticket)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));
        TicketId = ticket.Id;
        Titulo = ticket.Titulo;
        CreadoByUserId = ticket.CreadoByUserId;
        OccurredOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private TicketCreadoEvent(
        DateTime occurredOn,
        Guid ticketId,
        string titulo,
        Guid creadoByUserId)
    {
        OccurredOn = occurredOn;
        TicketId = ticketId;
        Titulo = titulo;
        CreadoByUserId = creadoByUserId;
    }
}