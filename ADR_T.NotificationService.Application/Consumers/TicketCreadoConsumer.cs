using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using ADR_T.NotificationService.Application.Persistence;
using ADR_T.NotificationService.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Events;

namespace ADR_T.NotificationService.Application.Consumers;
public class TicketCreadoConsumer : IConsumer<TicketCreadoEvent>
{
    private readonly ILogger<TicketCreadoConsumer> _logger;
    private readonly INotificationUnitOfWork _notificationContext;

    public TicketCreadoConsumer(ILogger<TicketCreadoConsumer> logger,
                                INotificationUnitOfWork notificationContext)
    {
        _logger = logger;
        _notificationContext = notificationContext;
    }
    public async Task Consume(ConsumeContext<TicketCreadoEvent> context)
    {
        var evento = context.Message;
        string logMessage = $"TicketCreadoEvent recibido: {evento.TicketId} - {evento.Titulo} - Creado por: {evento.CreadoByUserId}";

        _logger.LogInformation(logMessage);

        var notification = new NotificationLog
        {
            EventType = nameof(TicketCreadoEvent),
            Message = logMessage,
            DetailsJson = JsonSerializer.Serialize(evento),
            IsProcessed = true,
            Timestamp = DateTime.UtcNow
        };
        
        await _notificationContext.SaveAsync(notification, context.CancellationToken);
        _logger.LogInformation($"NotificationLog guardado: {notification.Id} - {notification.Message}");
    }
}

