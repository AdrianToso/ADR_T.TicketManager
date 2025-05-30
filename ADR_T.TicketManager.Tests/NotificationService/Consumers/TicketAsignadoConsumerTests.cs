// src\ADR_T.TicketManager.Tests\NotificationService\Consumers\TicketAsignadoConsumerTests.cs
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
using ADR_T.TicketManager.Core.Domain.Entities; // Necesario para Ticket
using ADR_T.TicketManager.Core.Domain.Enums; // Necesario para Enums

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
        _consumer = new TicketAsignadoConsumer(_loggerMock.Object, _notificationUnitOfWorkMock.Object);
    }

    private Ticket CreateMockTicket(Guid id, string titulo)
    {
        // Usar constructor real si es posible y sensible
        return new Ticket(id, titulo, "Desc", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid());
    }

    // Helper para crear un Usuario (ya no se necesita para simular email nulo en el evento)
    private User CreateUser(Guid id, string email, string name = "Usuario")
    {
        // Usar constructor real
        return new User(name, email, "hash") { Id = id };
    }


    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification_WithEmail()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorId = Guid.NewGuid();
        var email = "tecnico@example.com";
        var titulo = "Ticket Asignado";

        var mockTicket = CreateMockTicket(ticketId, titulo);
        var assignedUser = CreateUser(tecnicoId, email); // Usar el helper para crear usuario

        // Usar el constructor PÚBLICO del evento
        var ticketEvent = new TicketAsignadoEvent(mockTicket, assignedUser, asignadorId);


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
        // Verificar mensaje
        string expectedLogMessage = $"Ticket Asignado: ID={ticketEvent.TicketId} - Titulo: {ticketEvent.Titulo} , TecnicoID={ticketEvent.AsignadoUserId}, Email= {ticketEvent.AsignadoUserMail ?? "N/A"}.";
        Assert.Equal(expectedLogMessage, savedNotification.Message);
        Assert.Contains($"Email= {email}", savedNotification.Message); // Verificar email presente
        Assert.True(savedNotification.IsProcessed);

        // Verificar DetailsJson
        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.AsignadoUserId, deserializedDetails.AsignadoUserId);
        Assert.Equal(ticketEvent.AsignadoUserMail, deserializedDetails.AsignadoUserMail);
        Assert.Equal(ticketEvent.AsignadorUserId, deserializedDetails.AsignadorUserId);


        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning, // Verifica el nivel de log
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ticket Asignado: ID={ticketId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification_WithoutEmail()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorId = Guid.NewGuid();
        var titulo = "Ticket Asignado Sin Mail";

        // CREAR UN MOCK DEL EVENTO EN LUGAR DE INSTANCIAR LA CLASE REAL
        var mockTicketEvent = new Mock<TicketAsignadoEvent>(); // Crear mock del evento

        // Configurar las propiedades del mock del evento usando SetupGet
        mockTicketEvent.SetupGet(e => e.TicketId).Returns(ticketId);
        mockTicketEvent.SetupGet(e => e.Titulo).Returns(titulo);
        mockTicketEvent.SetupGet(e => e.AsignadoUserId).Returns(tecnicoId);
        mockTicketEvent.SetupGet(e => e.AsignadoUserMail).Returns((string)null); // <--- Configurar email nulo en el mock
        mockTicketEvent.SetupGet(e => e.AsignadorUserId).Returns(asignadorId);
        mockTicketEvent.SetupGet(e => e.OccurredOn).Returns(DateTime.UtcNow); // Configurar OccurredOn si es necesario


        var consumeContextMock = new Mock<ConsumeContext<TicketAsignadoEvent>>();
        consumeContextMock.Setup(ctx => ctx.Message).Returns(mockTicketEvent.Object); // Devolver el objeto mock
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
        // Verificar mensaje - Usar las propiedades del mock directamente para construir el mensaje esperado
        string expectedLogMessage = $"Ticket Asignado: ID={mockTicketEvent.Object.TicketId} - Titulo: {mockTicketEvent.Object.Titulo} , TecnicoID={mockTicketEvent.Object.AsignadoUserId}, Email= {mockTicketEvent.Object.AsignadoUserMail ?? "N/A"}.";
        Assert.Equal(expectedLogMessage, savedNotification.Message);
        Assert.Contains($"Email= N/A", savedNotification.Message); // Verifica N/A
        Assert.True(savedNotification.IsProcessed);

        // Verificar DetailsJson - Puede que esto falle si JsonSerializer no puede serializar un mock.
        // Si falla, se podría necesitar un enfoque diferente para verificar la serialización o aceptar esta limitación en el test unitario del consumidor.
        // Si es crucial probar la serialización de un evento con email nulo, se debería hacer un test unitario específico para la serialización del evento, no aquí.
        // Por ahora, mantendremos la verificación de serialización, pero sé consciente de que puede necesitar ajuste si JsonSerializer no maneja mocks.
        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Null(deserializedDetails.AsignadoUserMail); // Verificar null en el DTO serializado
        Assert.Equal(mockTicketEvent.Object.TicketId, deserializedDetails.TicketId);


        _loggerMock.Verify(
           x => x.Log(
               LogLevel.Warning,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Email= N/A")),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}