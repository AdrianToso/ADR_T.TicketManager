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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<ILogger<UpdateTicketCommandHandler>> _loggerMock;
    private readonly UpdateTicketCommandHandler _handler;

    public UpdateTicketCommandHandlerUnitTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _loggerMock = new Mock<ILogger<UpdateTicketCommandHandler>>();

        _unitOfWorkMock.Setup(uow => uow.Tickets).Returns(_ticketRepositoryMock.Object);

        _handler = new UpdateTicketCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTicket_WhenTicketExists()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var command = new UpdateTicketCommand
        {
            Id = ticketId,
            Titulo = "Nuevo Titulo",
            Descripcion = "Nueva Descripcion",
            Status = TicketStatus.EnProgreso,
            Prioridad = TicketPriority.Media,
            CreadoByUserId = creatorUserId
        };

        var existingTicket = new Ticket(ticketId, "Titulo Viejo", "Desc Vieja", TicketStatus.Abierto, TicketPriority.Alta, creatorUserId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(existingTicket);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ticketRepositoryMock.Verify(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Assert
        Assert.Equal(command.Titulo, existingTicket.Titulo);
        Assert.Equal(command.Descripcion, existingTicket.Descripcion);
        Assert.Equal(command.Status, existingTicket.Status);
        Assert.Equal(command.Prioridad, existingTicket.Priority);

        Assert.Equal(Unit.Value, result);

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
        };

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Ticket?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal($"El ticket con ID '{ticketId}' no existe.", exception.Message);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}