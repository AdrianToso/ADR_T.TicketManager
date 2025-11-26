using ADR_T.TicketManager.Core.Domain.Exceptions;

namespace ADR_T.TicketManager.Core.Domain.Entities;
public class User : EntityBase
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Ticket> TicketsCreados { get; private set; } = new List<Ticket>();
    public User() { }
    public User(string userName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("El nombre de usuario no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email no puede estar vacío.");

        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
    }
}

