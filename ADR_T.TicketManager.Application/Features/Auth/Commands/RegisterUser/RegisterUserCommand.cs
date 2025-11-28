using MediatR;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
public sealed record RegisterUserCommand(
    string Email,
    string Password
    ) : IRequest<Guid>;
