namespace ADR_T.TicketManager.Core.Domain.Entities;
public class TicketComment : EntityBase
{
    public string Comentario { get; private set; }
    public Guid TicketId { get; private set; }
    public Ticket Ticket { get; set; }
    public Guid AutorId { get; set; }
    public User Autor { get; set; }

    public TicketComment(string comentario, Guid ticketId, Guid autorId)
    {
        if(string.IsNullOrWhiteSpace(comentario))
            throw new ArgumentException("El comentario no puede estar vacío.", nameof(comentario));
        Comentario = comentario;
        TicketId = ticketId;
        AutorId = autorId;
    }
}
