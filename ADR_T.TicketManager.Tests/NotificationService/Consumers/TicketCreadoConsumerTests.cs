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
using System.Text.Json;
using System.Threading;
using ADR_T.TicketManager.Core.Domain.Entities; 
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.NotificationService.Consumers;

public class TicketCreadoConsumerTests
{
    private readonly Mock<ILogger<TicketCreadoConsumer>> _loggerMock;
    private readonly Mock<INotificationUnitOfWork> _notificationUnitOfWorkMock;
    private readonly TicketCreadoConsumer _consumer;

    public TicketCreadoConsumerTests()
    {
        _loggerMock = new Mock<ILogger<TicketCreadoConsumer>>();
        _notificationUnitOfWorkMock = new Mock<INotificationUnitOfWork>();
        _consumer = new TicketCreadoConsumer(_loggerMock.Object, _notificationUnitOfWorkMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var titulo = "Ticket Creado Test";

        var mockTicket = new Ticket(ticketId, titulo, "Desc", TicketStatus.Abierto, TicketPriority.Media, userId);

        var ticketEvent = new TicketCreadoEvent(mockTicket);

        var consumeContextMock = new Mock<ConsumeContext<TicketCreadoEvent>>();
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
        Assert.Equal(nameof(TicketCreadoEvent), savedNotification.EventType);
        string expectedLogMessage = $"TicketCreadoEvent recibido: {ticketEvent.TicketId} - {ticketEvent.Titulo} - Creado por: {ticketEvent.CreadoByUserId}";
        Assert.Equal(expectedLogMessage, savedNotification.Message);
        Assert.True(savedNotification.IsProcessed);

        var deserializedDetails = JsonSerializer.Deserialize<TicketCreadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.CreadoByUserId, deserializedDetails.CreadoByUserId);


        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"TicketCreadoEvent recibido: {ticketId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}