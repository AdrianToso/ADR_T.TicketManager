using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignTicketCommandHandler> _logger;


    public AssignTicketCommandHandler(IUnitOfWork unitOfWork, ILogger<AssignTicketCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Unit> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando asignación de Ticket ID: {TicketId} a Técnico ID: {TecnicoId} por Usuario ID: {AsignadorUserId}",
            request.TicketId, request.TecnicoId, request.AsignadorUserId);

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Asignación fallida: Ticket no encontrado (ID: {TicketId}).", request.TicketId);
            throw new DomainException($"El ticket con ID '{request.TicketId}' no existe.");
        }

        var tecnico = await _unitOfWork.Users.GetByIdAsync(request.TecnicoId);
        if (tecnico == null)
        {
            _logger.LogWarning("Asignación fallida: Técnico no encontrado (ID: {TecnicoId}).", request.TecnicoId);
            throw new DomainException($"El técnico con ID '{request.TecnicoId}' no existe.");
        }


        try
        {
            ticket.Asignar(tecnico);
            _logger.LogInformation("Ticket ID: {TicketId} preparado para asignación. Estado cambiado a {Status}. Evento de dominio añadido.", ticket.Id, ticket.Status);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Ticket ID: {TicketId} asignado exitosamente a Técnico ID: {TecnicoId}. Cambios guardados y evento despachado.", request.TicketId, request.TecnicoId);
        }
        catch (DomainException dex)
        {
            _logger.LogWarning(dex, "Error de dominio durante la asignación del Ticket ID: {TicketId}.", request.TicketId);
            throw;
        }
        catch (PersistenceException pex)
        {
            _logger.LogError(pex, "Error de persistencia durante la asignación del Ticket ID: {TicketId}.", request.TicketId);
            throw;
        }

        return Unit.Value;
    }
}