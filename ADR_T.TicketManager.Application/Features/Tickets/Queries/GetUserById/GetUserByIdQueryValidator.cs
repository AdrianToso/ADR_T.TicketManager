using FluentValidation;

namespace ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("El ID del Usuario es obligatorio.");
        }
    }
}