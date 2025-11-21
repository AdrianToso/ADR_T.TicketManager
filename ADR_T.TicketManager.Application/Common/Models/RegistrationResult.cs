namespace ADR_T.TicketManager.Application.Common.Models;
public sealed record RegistrationResult(bool Succeeded, Guid UserId = default, IEnumerable<string> Errors = null);