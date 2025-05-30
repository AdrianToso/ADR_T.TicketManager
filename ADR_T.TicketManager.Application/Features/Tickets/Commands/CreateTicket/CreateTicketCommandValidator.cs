using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("El Titulo es obligatorio.")
            .MaximumLength(100)
            .WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("La Descripcion es obligatoria.");
            
        RuleFor(x => x.Prioridad)
            .NotNull()
            .WithMessage("Prioridad es obligatoria.");

        RuleFor(x => x.CreadoByUserId)
            .NotEmpty();

    }
}
