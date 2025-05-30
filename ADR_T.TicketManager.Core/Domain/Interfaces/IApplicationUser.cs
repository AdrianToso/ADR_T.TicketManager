namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface IApplicationUser
{
    Guid Id { get; }
    string UserName { get; }
    string Email { get; }
}
