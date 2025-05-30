using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email es requerido.")
            .EmailAddress().WithMessage("Formato de mail invalido");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido.");
          
    }
}
