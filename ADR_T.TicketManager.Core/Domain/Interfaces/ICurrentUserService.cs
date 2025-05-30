namespace ADR_T.TicketManager.Core.Domain.Interfaces;
public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    List<string> Roles { get; }
    bool IsAdmin { get; }
}
