using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using ADR_T.NotificationService.Application.Persistence;
using ADR_T.NotificationService.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Events;

namespace ADR_T.NotificationService.Application.Consumers;
public class TicketActualizadoConsumer : IConsumer<TicketActualizadoEvent>
{
    private readonly ILogger<TicketActualizadoConsumer> _logger;
    private readonly INotificationUnitOfWork _notificationContext;

    public TicketActualizadoConsumer(ILogger<TicketActualizadoConsumer> logger,
                                     INotificationUnitOfWork notificationContext)
    {
        _logger = logger;
        _notificationContext = notificationContext;
    }
    public async Task Consume(ConsumeContext<TicketActualizadoEvent> context)
    {
        var evento = context.Message;

        string logMessage = $"Ticket Actualizado: ID={evento.TicketId} - Titulo: {evento.Titulo}. " +
                            $"Estado: '{evento.StatusAnterior}' -> '{evento.StatusNuevo}'. " +
                            $"Prioridad: '{evento.PrioridadAnterior}' -> '{evento.PrioridadNueva}'. " +
                            $"Actualizado por UserId: {evento.ActualizadoPorUserId}.";

        _logger.LogInformation(logMessage);

        var notification = new NotificationLog
        {
            EventType = nameof(TicketActualizadoEvent),
            Message = logMessage,
            DetailsJson = JsonSerializer.Serialize(evento),
            IsProcessed = true,
            Timestamp = DateTime.UtcNow
        };

        await _notificationContext.SaveAsync(notification, context.CancellationToken);
        _logger.LogInformation($"NotificationLog guardado para TicketActualizadoEvent: {notification.Id} - {notification.Message}");
    }
}
