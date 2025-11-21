namespace ADR_T.TicketManager.Application.Common.Models;
public sealed record UserDetailsResult(bool Succeeded, Guid UserId = default, string UserName = null, string Email = null);