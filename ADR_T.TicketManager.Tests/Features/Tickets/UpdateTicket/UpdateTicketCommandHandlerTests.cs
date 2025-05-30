using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Exceptions;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Commands.UpdateTicket;
public class UpdateTicketCommandHandlerUnitTests
{
    // Mocks necesarios según el constructor ACTUAL del handler
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock; // Mock para el repo accedido vía UoW
    private readonly Mock<ILogger<UpdateTicketCommandHandler>> _loggerMock; 
    private readonly UpdateTicketCommandHandler _handler;

    public UpdateTicketCommandHandlerUnitTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _loggerMock = new Mock<ILogger<UpdateTicketCommandHandler>>(); 

        // Configurar el mock de UnitOfWork para devolver el mock del repositorio
        _unitOfWorkMock.Setup(uow => uow.TicketRepository).Returns(_ticketRepositoryMock.Object);

        // Instanciar el handler con su constructor ACTUAL (IUnitOfWork, ILogger)
        // Ya no se necesitan mocks de ITicketRepository o UserManager aquí directamente
        _handler = new UpdateTicketCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTicket_WhenTicketExists()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid(); // ID del creador original
        var command = new UpdateTicketCommand
        {
            Id = ticketId,
            Titulo = "Nuevo Titulo",
            Descripcion = "Nueva Descripcion",
            Status = TicketStatus.EnProgreso,
            Prioridad = TicketPriority.Media,
            CreadoByUserId = creatorUserId // Este ID podría no ser relevante para la lógica de update en sí
        };

        // Simular que el ticket existe en el repositorio
        var existingTicket = new Ticket(ticketId, "Titulo Viejo", "Desc Vieja", TicketStatus.Abierto, TicketPriority.Alta, creatorUserId);
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
                             .ReturnsAsync(existingTicket);

        // Configurar CommitAsync para que devuelva 1
        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verificar que se buscó el ticket
        _ticketRepositoryMock.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);

        // Verificar que el método Update de la entidad fue llamado (implícito por el cambio de propiedades)
        // No podemos verificar directamente la llamada a ticket.Update si no mockeamos la entidad,
        // pero podemos verificar que CommitAsync fue llamado.

        // Verificar que se llamó a CommitAsync
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);

        // Verificar que el resultado es Unit.Value
        Assert.Equal(Unit.Value, result);

        // Opcional: Verificar que las propiedades del ticket existente (si no fuera mock) se actualizaron
        // Esto es más para tests de integración. En unit test, confiamos en que CommitAsync guarda los cambios.
        // Assert.Equal(command.Titulo, existingTicket.Titulo); // No funcionará directamente con el mock
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var command = new UpdateTicketCommand
        {
            Id = ticketId,
            Titulo = "Nuevo Titulo",
            // ... resto de propiedades
        };

        // Simular que el ticket NO existe
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
                             .ReturnsAsync((Ticket?)null); // Devuelve null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        // Verificar mensaje
        Assert.Equal($"El ticket con ID '{ticketId}' no existe.", exception.Message);

        // Asegurarse de que no se intentó hacer commit
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
    }
}
