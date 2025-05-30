using MediatR;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;

    public sealed record LoginUserCommand(
        string Email,
        string Password
    ) : IRequest<LoginResponse>;

public record LoginResponse(string Token, Guid UserId);
