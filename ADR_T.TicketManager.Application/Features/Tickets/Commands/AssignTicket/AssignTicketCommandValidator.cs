using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("El ID del Ticket es obligatorio.");

        RuleFor(x => x.TecnicoId)
            .NotEmpty().WithMessage("El ID del Técnico es obligatorio.");

        RuleFor(x => x.AsignadorUserId)
           .NotEmpty().WithMessage("El ID del usuario que asigna es obligatorio.");
    }
}
