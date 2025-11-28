using Moq;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketStatistics;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetTicketStatistics;

public class GetTicketStatisticsQueryHandlerTests
{
    private readonly Mock<IRepository<Ticket>> _ticketRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetTicketStatisticsQueryHandler>> _loggerMock;
    private readonly GetTicketStatisticsQueryHandler _handler;

    public GetTicketStatisticsQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IRepository<Ticket>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetTicketStatisticsQueryHandler>>();
        _handler = new GetTicketStatisticsQueryHandler(
            _ticketRepositoryMock.Object,
            _loggerMock.Object,
            _userRepositoryMock.Object);
    }

    private User CreateUserTecnico(Guid id, string name, string email)
    {
        return new User(name, email, "fakehash") { Id = id };
    }

    //[Fact]
    //public async Task Handle_ShouldReturnCorrectStatistics_WhenTicketsExist()
    //{
    //    // Arrange
    //    var query = new GetTicketStatisticsQuery();
    //    var userId1 = Guid.NewGuid();
    //    var userId2 = Guid.NewGuid();
    //    var tecnicoId1 = Guid.NewGuid();
    //    var tecnicoId2 = Guid.NewGuid();

    //    var userTecnico1 = CreateUserTecnico(tecnicoId1, "Tecnico Uno", "tecnico1@test.com");
    //    var userTecnico2 = CreateUserTecnico(tecnicoId2, "Tecnico Dos", "tecnico2@test.com");

    //    var now = DateTime.UtcNow;

    //    // Tickets
    //    var ticket1 = new Ticket(Guid.NewGuid(), "T1", "D1", TicketStatus.Abierto, TicketPriority.Media, userId1) { FechacCreacion = now };
    //    var ticket2 = new Ticket(Guid.NewGuid(), "T2", "D2", TicketStatus.EnProgreso, TicketPriority.Alta, userId1) { FechacCreacion = now };
    //    ticket2.Asignar(userTecnico1);
    //    var ticket3 = new Ticket(Guid.NewGuid(), "T3", "D3", TicketStatus.Resuelto, TicketPriority.Alta, userId2) { FechacCreacion = now, FechacActualizacion = now.AddHours(5) };
    //    ticket3.Asignar(userTecnico1);

    //    var ticket4 = new Ticket(Guid.NewGuid(), "T4", "D4", TicketStatus.Abierto, TicketPriority.Baja, userId1) { FechacCreacion = now };
    //    ticket4.Asignar(userTecnico2); 

    //    var ticket5 = new Ticket(Guid.NewGuid(), "T5", "D5", TicketStatus.Abierto, TicketPriority.Critica, userId2) { FechacCreacion = now };
    //    var ticket6 = new Ticket(Guid.NewGuid(), "T6", "D6", TicketStatus.Resuelto, TicketPriority.Media, userId2) { FechacCreacion = now, FechacActualizacion = now.AddHours(60) };
    //    ticket6.Asignar(userTecnico2);

    //    var tickets = new List<Ticket> { ticket1, ticket2, ticket3, ticket4, ticket5, ticket6 };

    //    // Mocks
    //    _ticketRepositoryMock.Setup(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(tickets);

    //    var tecnicosFromRepo = new List<User> { userTecnico1, userTecnico2 };
    //    var tecnicoIds = tickets.Where(t => t.AsignadoUserId.HasValue).Select(t => t.AsignadoUserId.Value).Distinct().ToList();

    //    _userRepositoryMock.Setup(repo => repo.GetByIdsAsync(
    //        It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(tecnicoIds)),
    //        It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(tecnicosFromRepo);

    //    // Act
    //    var result = await _handler.Handle(query, CancellationToken.None);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(6, result.TotalTickets);

    //    Assert.Equal(3, result.TicketsAbiertos);
    //    Assert.Equal(1, result.TicketsEnProgreso);
    //    Assert.Equal(2, result.TicketsResueltos);
    //    Assert.Equal(0, result.TicketsCerrados);

    //    Assert.Equal(1, result.TicketsPorPrioridad.GetValueOrDefault(TicketPriority.Baja.ToString()));

    //    Assert.Equal(2, result.TicketsPorPrioridad.GetValueOrDefault(TicketPriority.Media.ToString())); 
    //    Assert.Equal(2, result.TicketsPorPrioridad.GetValueOrDefault(TicketPriority.Alta.ToString())); 
    //    Assert.Equal(1, result.TicketsPorPrioridad.GetValueOrDefault(TicketPriority.Critica.ToString())); 

    //    Assert.Equal(2, result.TicketsPorTecnico["Tecnico Uno"]); 
    //    Assert.Equal(2, result.TicketsPorTecnico["Tecnico Dos"]); 

    //    Assert.Equal(2, result.TiemposDeResolucion.Count);
    //    Assert.Equal(1, result.TiemposDeResolucion.GetValueOrDefault("0-24 horas")); 
    //    Assert.Equal(1, result.TiemposDeResolucion.GetValueOrDefault("Más de 48 horas")); 


    //    _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    //    _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    //}

    [Fact]
    public async Task Handle_ShouldReturnEmptyStatistics_WhenNoTicketsExist()
    {
        // Arrange
        var query = new GetTicketStatisticsQuery();
        var emptyTickets = new List<Ticket>();

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emptyTickets);

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

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTicketRepositoryThrows()
    {
        // Arrange
        var query = new GetTicketStatisticsQuery();
        var dbException = new Exception("Database connection error");

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(dbException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
           _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Ocurrió un error al obtener los datos para las estadísticas.", exception.Message);
        Assert.Equal(dbException, exception.InnerException);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}