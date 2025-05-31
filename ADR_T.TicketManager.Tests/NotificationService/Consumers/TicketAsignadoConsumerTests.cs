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
using ADR_T.TicketManager.Core.Domain.Entities; // Necesario para Ticket y User
using ADR_T.TicketManager.Core.Domain.Enums; // Necesario para Enums
using System.Linq; // Puede que necesites este si lo usas en helpers

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

    // Helper para crear un Ticket mockeado con los argumentos que tu constructor real de Ticket espera
    private Ticket CreateMockTicket(Guid id, string titulo)
    {
        // AJUSTA ESTE CONSTRUCTOR para que coincida exactamente con tu constructor real de Ticket
        // Si tu constructor de Ticket necesita más argumentos (ej. descripcion, estado, prioridad, creadorId),
        // añádelos aquí con valores de prueba.
        return new Ticket(id, titulo, "Descripción de prueba", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid());
    }

    // Helper para crear un User válido (con email no nulo, como exige tu dominio)
    private User CreateUser(Guid id, string email, string name = "Usuario")
    {
        // AJUSTA ESTE CONSTRUCTOR para que coincida exactamente con tu constructor real de User
        // Asumo que tu constructor de User necesita userName, email, passwordHash.
        return new User(name, email, "hash") { Id = id };
    }


    [Fact]
    public async Task Consume_ShouldLogAndSaveNotification_WithEmail()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid(); // Este es el ID del usuario asignado
        var email = "tecnico@example.com";
        var titulo = "Ticket Asignado";

        var mockTicket = CreateMockTicket(ticketId, titulo);
        var assignedUser = CreateUser(tecnicoId, email); // Usuario asignado con email válido

        // Creamos el evento TicketAsignadoEvent usando el constructor de 2 argumentos (Ticket, User)
        // Este es el constructor que existe en tu dominio y se llama desde Ticket.Asignar()
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

        // Verificamos las partes clave del mensaje
        Assert.Contains($"Ticket Asignado: ID={ticketId}", savedNotification.Message);
        Assert.Contains($"Titulo: {titulo}", savedNotification.Message);
        Assert.Contains($"TecnicoID={tecnicoId}", savedNotification.Message);
        Assert.Contains($"Email= {email}", savedNotification.Message); // Esperamos el email real

        Assert.True(savedNotification.IsProcessed);

        // Verificar DetailsJson
        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.AsignadoUserId, deserializedDetails.AsignadoUserId);
        Assert.Equal(ticketEvent.AsignadoUserMail, deserializedDetails.AsignadoUserMail);
        // Si tu evento tiene Id y OccurredOn (de la clase base), puedes asertarlos si lo deseas:
        // Assert.Equal(ticketEvent.Id, deserializedDetails.Id);
        // Assert.Equal(ticketEvent.OccurredOn, deserializedDetails.OccurredOn);


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
    // Justificación del cambio de nombre: Este test ya no prueba el escenario "sin email",
    // porque tu dominio no permite crear un evento con un email nulo.
    // Ahora prueba un segundo escenario donde el usuario asignado tiene un email válido.
    public async Task Consume_ShouldLogAndSaveNotification_WithValidAssignedUser()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var titulo = "Ticket Asignado con Usuario Válido";
        var email = "otro.tecnico.valido@example.com"; // Aquí usamos un email válido

        // Justificación: Debido a las restricciones de tu dominio (User.Email no nulo y Evento sealed),
        // no podemos generar un TicketAsignadoEvent con un email nulo.
        // Por lo tanto, este test verificará el procesamiento de un evento con un usuario asignado válido.
        var mockTicket = CreateMockTicket(ticketId, titulo);
        var assignedUser = CreateUser(tecnicoId, email); // Creamos un User con un email válido

        // Creamos el evento TicketAsignadoEvent usando el constructor de 2 argumentos (Ticket, User)
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

        // Verificamos las partes clave del mensaje
        Assert.Contains($"Ticket Asignado: ID={ticketId}", savedNotification.Message);
        Assert.Contains($"Titulo: {titulo}", savedNotification.Message);
        Assert.Contains($"TecnicoID={tecnicoId}", savedNotification.Message);
        Assert.Contains($"Email= {email}", savedNotification.Message); // Esperamos el email real del usuario válido

        Assert.True(savedNotification.IsProcessed);

        // Verificar DetailsJson
        var deserializedDetails = JsonSerializer.Deserialize<TicketAsignadoEvent>(savedNotification.DetailsJson);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(ticketEvent.AsignadoUserMail, deserializedDetails.AsignadoUserMail); // El email será válido
        Assert.Equal(ticketEvent.TicketId, deserializedDetails.TicketId);
        Assert.Equal(ticketEvent.Titulo, deserializedDetails.Titulo);
        Assert.Equal(ticketEvent.AsignadoUserId, deserializedDetails.AsignadoUserId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Email= {email}")), // Verificamos que el log contenga el email real
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}