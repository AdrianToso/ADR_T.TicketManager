using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Events;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;
public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTicketCommandHandler> _logger;
    public CreateTicketCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateTicketCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.CreadoByUserId);
        if (user == null)
            throw new DomainException($"El usuario con ID '{request.CreadoByUserId}' no existe.");


        var ticket = new Ticket(
            request.Titulo,
            request.Descripcion,
            TicketStatus.Abierto,
            request.Prioridad,
            user.Id 
            );
        ticket.AddDomainEvent(new TicketCreadoEvent(ticket));

        await _unitOfWork.TicketRepository.AddAsync(ticket);

        await _unitOfWork.CommitAsync();

        return ticket.Id;
    }
}