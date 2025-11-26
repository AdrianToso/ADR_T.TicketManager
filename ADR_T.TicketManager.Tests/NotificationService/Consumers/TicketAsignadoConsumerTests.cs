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
using System.Linq;

namespace ADR_T.TicketManager.Tests.NotificationService.Consumers;

public class TicketAsignadoConsumerTests
{
    private readonly Mock<ILogger<TicketAsignadoConsumer>> _loggerMock;
    private readonly Mock<INotificationUnitOfWork> _notificationUnitOfWorkMock;
    private readonly TicketAsignadoConsumer _consumer;

    public TicketAsignadoConsumerTests()
    {
        _loggerMock = new Mock<ILogger<TicketAsignadoConsumer>>();
        _notificationUnitOfWorkMock = new Mock<INotificationUnitOfWork>();
        _consumer = new TicketAsignadoConsumer(
            _loggerMock.Object,
            _notificationUnitOfWorkMock.Object);
    }

    private Ticket CreateMockTicket(Guid id, string titulo)
    {
        return new Ticket(id, titulo, "Descripción de prueba", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid());
    }

    private User CreateUser(Guid id, string email, string name = "Usuario")
    {
        return new User(name, email, "hash") { Id = id };
    }


    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification_WithEmail()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var email = "tecnico@example.com";
        var titulo = "Ticket Asignado";

        var mockTicket = CreateMockTicket(ticketId, titulo);
        var assignedUser = CreateUser(tecnicoId, email);

        var ticketEvent = new TicketAsignadoEvent(mockTicket, assignedUser);


        var consumeContextMock = new Mock<ConsumeContext<TicketAsignadoEvent>>();
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
        Assert.Equal(nameof(TicketAsignadoEvent), savedNotification.EventType);

        Assert.Contains($"Ticket Asignado: ID={ticketId}", savedNotification.Message);
        Assert.Contains($"Titulo: {titulo}", savedNotification.Message);
        Assert.Contains($"TecnicoID={tecnicoId}", savedNotification.Message);
        Assert.Contains($"Email= {email}", savedNotification.Message);

        Assert.True(savedNotification.IsProcessed);

        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.AsignadoUserId, deserializedDetails.AsignadoUserId);
        Assert.Equal(ticketEvent.AsignadoUserMail, deserializedDetails.AsignadoUserMail);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ticket Asignado: ID={ticketId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification_WithValidAssignedUser()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var titulo = "Ticket Asignado con Usuario Válido";
        var email = "otro.tecnico.valido@example.com";

        var mockTicket = CreateMockTicket(ticketId, titulo);
        var assignedUser = CreateUser(tecnicoId, email);

        var ticketEvent = new TicketAsignadoEvent(mockTicket, assignedUser);


        var consumeContextMock = new Mock<ConsumeContext<TicketAsignadoEvent>>();
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
        Assert.Equal(nameof(TicketAsignadoEvent), savedNotification.EventType);

        Assert.Contains($"Ticket Asignado: ID={ticketId}", savedNotification.Message);
        Assert.Contains($"Titulo: {titulo}", savedNotification.Message);
        Assert.Contains($"TecnicoID={tecnicoId}", savedNotification.Message);
        Assert.Contains($"Email= {email}", savedNotification.Message);

        Assert.True(savedNotification.IsProcessed);

        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.AsignadoUserMail, deserializedDetails.AsignadoUserMail);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.AsignadoUserId, deserializedDetails.AsignadoUserId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Email= {email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}