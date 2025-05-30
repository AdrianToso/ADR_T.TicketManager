using Moq;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketStatistics;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetTicketStatistics;

public class GetTicketStatisticsQueryHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetTicketStatisticsQueryHandler>> _loggerMock;
    private readonly GetTicketStatisticsQueryHandler _handler;

    public GetTicketStatisticsQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetTicketStatisticsQueryHandler>>();
        _handler = new GetTicketStatisticsQueryHandler(
            _ticketRepositoryMock.Object,
            _loggerMock.Object,
            _userRepositoryMock.Object);
    }

    // Helper para crear un mock/instancia de Usuario Técnico - USANDO INSTANCIA REAL
    private User CreateUserTecnico(Guid id, string name, string email) // Cambiado a CreateUserTecnico
    {
        // Usar constructor real de User, el cual hereda de EntityBase.
        // Proveer email para satisfacer la validación del constructor.
        return new User(name, email, "fakehash") { Id = id };
    }


    [Fact]
    public async Task Handle_ShouldReturnCorrectStatistics_WhenTicketsExist()
    {
        // Arrange
        var query = new GetTicketStatisticsQuery();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var tecnicoId1 = Guid.NewGuid();
        var tecnicoId2 = Guid.NewGuid();

        // Usar el helper corregido para crear instancias REALES
        var userTecnico1 = CreateUserTecnico(tecnicoId1, "Tecnico Uno", "tecnico1@test.com");
        var userTecnico2 = CreateUserTecnico(tecnicoId2, "Tecnico Dos", "tecnico2@test.com");

        var now = DateTime.UtcNow;

        // Crear tickets y luego asignarlos si corresponde
        var ticket1 = new Ticket("T1", "D1", TicketStatus.Abierto, TicketPriority.Media, userId1) { FechacCreacion = now };
        var ticket2 = new Ticket("T2", "D2", TicketStatus.EnProgreso, TicketPriority.Alta, userId1) { FechacCreacion = now };
        ticket2.Asignar(userTecnico1); // Asignar correctamente
        var ticket3 = new Ticket("T3", "D3", TicketStatus.Resuelto, TicketPriority.Alta, userId2) { FechacCreacion = now, FechacActualizacion = now.AddHours(5) };
        ticket3.Asignar(userTecnico1); // Asignar correctamente
        var ticket4 = new Ticket("T4", "D4", TicketStatus.Cerrado, TicketPriority.Baja, userId1) { FechacCreacion = now, FechacActualizacion = now.AddHours(30) };
        ticket4.Asignar(userTecnico2); // Asignar correctamente
        var ticket5 = new Ticket("T5", "D5", TicketStatus.Abierto, TicketPriority.Critica, userId2) { FechacCreacion = now };
        var ticket6 = new Ticket("T6", "D6", TicketStatus.Resuelto, TicketPriority.Media, userId2) { FechacCreacion = now, FechacActualizacion = now.AddHours(60) };
        ticket6.Asignar(userTecnico2); // Asignar correctamente

        var tickets = new List<Ticket> { ticket1, ticket2, ticket3, ticket4, ticket5, ticket6 };

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync()).ReturnsAsync(tickets);

        var tecnicosFromRepo = new List<User> { userTecnico1, userTecnico2 };
        var tecnicoIds = tickets.Where(t => t.AsignadoUserId.HasValue).Select(t => t.AsignadoUserId.Value).Distinct().ToList();
        _userRepositoryMock.Setup(repo => repo.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(tecnicoIds))))
                           .ReturnsAsync(tecnicosFromRepo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.TotalTickets);
        Assert.Equal(2, result.TicketsAbiertos);
        Assert.Equal(1, result.TicketsEnProgreso);
        Assert.Equal(2, result.TicketsResueltos);
        Assert.Equal(1, result.TicketsCerrados);

        // Por Prioridad
        Assert.Equal(1, result.TicketsPorPrioridad[TicketPriority.Baja.ToString()]);
        Assert.Equal(2, result.TicketsPorPrioridad[TicketPriority.Media.ToString()]);
        Assert.Equal(2, result.TicketsPorPrioridad[TicketPriority.Alta.ToString()]);
        Assert.Equal(1, result.TicketsPorPrioridad[TicketPriority.Critica.ToString()]);

        // Por Estado
        Assert.Equal(2, result.TicketsPorEstado[TicketStatus.Abierto.ToString()]);
        Assert.Equal(1, result.TicketsPorEstado[TicketStatus.EnProgreso.ToString()]);
        Assert.Equal(2, result.TicketsPorEstado[TicketStatus.Resuelto.ToString()]);
        Assert.Equal(1, result.TicketsPorEstado[TicketStatus.Cerrado.ToString()]);

        // Por Tecnico
        Assert.Equal(2, result.TicketsPorTecnico["Tecnico Uno"]);
        Assert.Equal(2, result.TicketsPorTecnico["Tecnico Dos"]);

        // Tiempos de Resolución (Solo T3, T4, T6 son Resuelto/Cerrado con FechaActualizacion)
        Assert.Equal(1, result.TiemposDeResolucion["0-24 horas"]);
        Assert.Equal(1, result.TiemposDeResolucion["24-48 horas"]);
        Assert.Equal(1, result.TiemposDeResolucion["Más de 48 horas"]);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(), Times.Once);
        // Esta verificación ahora pasará porque el helper usa User real y la propiedad Id es accesible.
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyStatistics_WhenNoTicketsExist()
    {
        // Arrange
        var query = new GetTicketStatisticsQuery();
        var emptyTickets = new List<Ticket>();

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync()).ReturnsAsync(emptyTickets);

        // ACT
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalTickets);
        Assert.Equal(0, result.TicketsAbiertos);
        Assert.Equal(0, result.TicketsEnProgreso);
        Assert.Equal(0, result.TicketsResueltos);
        Assert.Equal(0, result.TicketsCerrados);
        Assert.Empty(result.TicketsPorPrioridad);
        Assert.Empty(result.TicketsPorEstado);
        Assert.Empty(result.TicketsPorTecnico);
        Assert.Empty(result.TiemposDeResolucion);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(), Times.Once);
        // Esta verificación ahora pasará debido al cambio en el handler para no llamar GetByIdsAsync si no hay tickets asignados
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTicketRepositoryThrows()
    {
        // Arrange
        var query = new GetTicketStatisticsQuery();
        var dbException = new Exception("Database connection error");

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync()).ThrowsAsync(dbException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
           _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Ocurrió un error al obtener los datos para las estadísticas.", exception.Message);
        Assert.Equal(dbException, exception.InnerException);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }
}