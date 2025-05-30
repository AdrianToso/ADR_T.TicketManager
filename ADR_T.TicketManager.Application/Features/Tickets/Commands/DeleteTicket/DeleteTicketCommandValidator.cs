using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
public class DeleteTicketCommandValidator : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}
