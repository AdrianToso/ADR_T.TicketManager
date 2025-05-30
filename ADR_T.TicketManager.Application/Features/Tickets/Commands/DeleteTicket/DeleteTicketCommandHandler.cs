using MediatR;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
public sealed class DeleteTicketCommandHandler :IRequestHandler<DeleteTicketCommand, Unit>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId);
        if (ticket == null)
            throw new DomainException("El ticket no existe.");
        await _ticketRepository.DeleteAsync(ticket);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
