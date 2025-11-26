using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Events;
using ADR_T.NotificationService.Domain.Entities;
using ADR_T.NotificationService.Application.Persistence;

namespace ADR_T.NotificationService.Application.Consumers;
public class TicketAsignadoConsumer : IConsumer<TicketAsignadoEvent>
{
    private readonly ILogger<TicketAsignadoConsumer> _logger;
    private readonly INotificationUnitOfWork _notificationContext;

    public TicketAsignadoConsumer(ILogger<TicketAsignadoConsumer> logger,
                                     INotificationUnitOfWork notificationContext)
    {
        _logger = logger;
        _notificationContext = notificationContext;
    }

    public async Task Consume(ConsumeContext<TicketAsignadoEvent> context)
    {
        var evento = context.Message;
        string logMessage = $"Ticket Asignado: ID={evento.TicketId} - Titulo: {evento.Titulo} , TecnicoID={evento.AsignadoUserId}, Email= {evento.AsignadoUserMail ?? "N/A"}.";
        _logger.LogWarning(logMessage);

        var notification = new NotificationLog
        {
            EventType = nameof(TicketAsignadoEvent),
            Message = logMessage,
            DetailsJson = JsonSerializer.Serialize(evento),
            IsProcessed = true,
            Timestamp = DateTime.UtcNow
        };
        await _notificationContext.SaveAsync(notification, context.CancellationToken);
        _logger.LogInformation($"NotificationLog guardado: {notification.Id} - {notification.Message}");
    }
}