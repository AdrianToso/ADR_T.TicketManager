namespace ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;
public sealed record LoginResponse(string Token, Guid UserId);