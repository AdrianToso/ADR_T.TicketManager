using Xunit;
using Moq;
using MassTransit;
using ADR_T.NotificationService.Application.Consumers;
using ADR_T.NotificationService.Application.Persistence;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using ADR_T.TicketManager.Core.Domain.Events;
using ADR_T.NotificationService.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using System.Text.Json;
using System.Threading;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Tests.NotificationService.Consumers;

public class TicketActualizadoConsumerTests
{
    private readonly Mock<ILogger<TicketActualizadoConsumer>> _loggerMock;
    private readonly Mock<INotificationUnitOfWork> _notificationUnitOfWorkMock;
    private readonly TicketActualizadoConsumer _consumer;

    public TicketActualizadoConsumerTests()
    {
        _loggerMock = new Mock<ILogger<TicketActualizadoConsumer>>();
        _notificationUnitOfWorkMock = new Mock<INotificationUnitOfWork>();
        _consumer = new TicketActualizadoConsumer(_loggerMock.Object, _notificationUnitOfWorkMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var statusAnterior = TicketStatus.Abierto;
        var prioridadAnterior = TicketPriority.Baja;
        var statusNuevo = TicketStatus.EnProgreso;
        var prioridadNueva = TicketPriority.Media;


        var mockTicket = new Ticket(ticketId, "Ticket Actualizado", "Desc", statusNuevo, prioridadNueva, Guid.NewGuid());

        var ticketEvent = new TicketActualizadoEvent(
            mockTicket,
            statusAnterior,
            prioridadAnterior,
            userId
            );

        var consumeContextMock = new Mock<ConsumeContext<TicketActualizadoEvent>>();
        consumeContextMock.Setup(ctx => ctx.Message).Returns(ticketEvent);
        consumeContextMock.Setup(ctx => ctx.CancellationToken).Returns(CancellationToken.None);

        NotificationLog savedNotification = null;
        _notificationUnitOfWorkMock
            .Setup(uow => uow.SaveAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationLog, CancellationToken>((log, ct) => savedNotification = log)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationUnitOfWorkMock.Verify(uow => uow.SaveAsync(It.IsAny<NotificationLog>(), CancellationToken.None), Times.Once);

        Assert.NotNull(savedNotification);
        Assert.Equal(nameof(TicketActualizadoEvent), savedNotification.EventType);
        string expectedLogMessage = $"Ticket Actualizado: ID={ticketEvent.TicketId} - Titulo: {ticketEvent.Titulo}. " +
                                    $"Estado: '{ticketEvent.StatusAnterior}' -> '{ticketEvent.StatusNuevo}'. " +
                                    $"Prioridad: '{ticketEvent.PrioridadAnterior}' -> '{ticketEvent.PrioridadNueva}'. " +
                                    $"Actualizado por UserId: {ticketEvent.ActualizadoPorUserId}.";
        Assert.Equal(expectedLogMessage, savedNotification.Message);
        Assert.True(savedNotification.IsProcessed);

        var deserializedDetails = JsonSerializer.Deserialize<TicketActualizadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.StatusAnterior, deserializedDetails.StatusAnterior);
        Assert.Equal(ticketEvent.StatusNuevo, deserializedDetails.StatusNuevo);
        Assert.Equal(ticketEvent.PrioridadAnterior, deserializedDetails.PrioridadAnterior);
        Assert.Equal(ticketEvent.PrioridadNueva, deserializedDetails.PrioridadNueva);
        Assert.Equal(ticketEvent.ActualizadoPorUserId, deserializedDetails.ActualizadoPorUserId);


        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ticket Actualizado: ID={ticketId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}