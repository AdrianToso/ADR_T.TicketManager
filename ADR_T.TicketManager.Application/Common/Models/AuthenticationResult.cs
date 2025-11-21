namespace ADR_T.TicketManager.Application.Common.Models;
public sealed record AuthenticationResult(bool Succeeded, Guid UserId = default, string Token = null, IEnumerable<string> Errors = null);
