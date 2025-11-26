using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email es requerido.")
            .EmailAddress().WithMessage("Formato de mail invalido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido.")
            .MinimumLength(8).WithMessage("Password debe tener 8 caracteres como minimo.");
        //.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
        //.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
        //.Matches("[0-9]").WithMessage("Password must contain at least one number.")
        //.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
