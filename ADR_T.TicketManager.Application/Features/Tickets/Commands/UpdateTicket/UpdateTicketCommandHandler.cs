using MediatR;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTicketCommandHandler> _logger;

    public UpdateTicketCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTicketCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(request.Id);
        if (ticket == null)
        {
            _logger.LogWarning("Intento de actualizar ticket no existente: ID {TicketId}", request.Id);
            throw new DomainException($"El ticket con ID '{request.Id}' no existe.");
        }
                    
        _logger.LogInformation("Actualizando ticket ID {TicketId}. Titulo: {Titulo}, Status: {Status}, Prioridad: {Prioridad}",
            request.Id, request.Titulo, request.Status, request.Prioridad);

        ticket.Update(request.Titulo, request.Descripcion, request.Status, request.Prioridad, request.CreadoByUserId);

        // Guardar cambios en la base de datos y disparar eventos de dominio
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Ticket ID {TicketId} actualizado exitosamente.", request.Id);

        return Unit.Value;
    }
}