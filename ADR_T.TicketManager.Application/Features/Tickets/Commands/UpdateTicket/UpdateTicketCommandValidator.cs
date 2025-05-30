using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
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
            .IsInEnum()
            .WithMessage("Prioridad no válida.");
    }
}
