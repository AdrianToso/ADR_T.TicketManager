using MassTransit;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Infrastructure.Services;
public class RabbitMQEventBus : IEventBus
{
    private readonly IBus _bus;
    private readonly ILogger<RabbitMQEventBus> _logger;

    public RabbitMQEventBus(IBus bus, ILogger<RabbitMQEventBus> logger)
    {
        _bus = bus ?? throw new ArgumentException(nameof(bus));
        _logger = logger ?? throw new ArgumentException(nameof(logger));
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
       where T : class
    {
        // Obtiene el tipo de mensaje en tiempo de ejecución
        var actualType = message.GetType();
        var messageTypeName = actualType.FullName;

        try
        {
            _logger.LogInformation(
                "Publicando mensaje de tipo {MessageType} en RabbitMQ. Mensaje: {@Message}",
                messageTypeName, message);

            // Usa la sobrecarga que publica al exchange del tipo concreto
            await _bus.Publish(message, actualType, cancellationToken);

            _logger.LogInformation(
                "Mensaje de tipo {MessageType} publicado exitosamente.",
                messageTypeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al publicar mensaje de tipo {MessageType}. Mensaje: {@Message}",
                messageTypeName, message);
            throw;
        }
    }
}

