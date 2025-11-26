using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Events;
using ADR_T.TicketManager.Core.Domain.Exceptions;

namespace ADR_T.TicketManager.Core.Domain.Entities;
public class Ticket : EntityBase
{
    public string Titulo { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }

    public Guid CreadoByUserId { get; private set; }
    public User CreadoByUser { get; private set; } = null!;
    public Guid? AsignadoUserId { get; private set; } = null!;
    public User AsignadoUser { get; private set; } = null!;

    private readonly List<TicketComment> _comments = new();
    public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();
    #region constructores
    private Ticket() { }
    public Ticket(string titulo, string descripcion, TicketStatus status, TicketPriority priority, Guid creadoByUserId) : base()
    {
        Titulo = titulo;
        Descripcion = descripcion;
        Status = status;
        Priority = priority;
        CreadoByUserId = creadoByUserId;
        FechacCreacion = DateTime.UtcNow;
    }
    public Ticket(Guid id, string titulo, string descripcion, TicketStatus status, TicketPriority priority, Guid creadoByUserId) : base(id)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        Status = status;
        Priority = priority;
        CreadoByUserId = creadoByUserId;
    }


    #endregion

    public void Update(string titulo, string descripcion, TicketStatus status, TicketPriority priority, Guid actualizadoPorUserId)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("El título no puede estar vacío al actualizar.");
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción no puede estar vacía al actualizar.");

        var statusAnterior = this.Status;
        var prioridadAnterior = this.Priority;

        Titulo = titulo;
        Descripcion = descripcion;
        Status = status;
        Priority = priority;
        FechacActualizacion = DateTime.UtcNow;

        // Disparar evento de actualización con el estado anterior y el ID del usuario
        AddDomainEvent(new TicketActualizadoEvent(this, statusAnterior, prioridadAnterior, actualizadoPorUserId));
    }
    public void Asignar(User user)
    {
        if (user == null)
            throw new DomainException("El usuario no puede ser null.");

        AsignadoUserId = user.Id;
        Status = TicketStatus.Asignado;
        FechacActualizacion = DateTime.UtcNow;

        AddDomainEvent(new TicketAsignadoEvent(this, user));
    }

}
