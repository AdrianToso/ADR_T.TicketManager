using Moq;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Exceptions;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Commands.DeleteTicket;

public class DeleteTicketCommandHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteTicketCommandHandler _handler;

    public DeleteTicketCommandHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteTicketCommandHandler(_ticketRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Ticket CreateMockTicket(Guid id)
    {
        return new Ticket(id, "Titulo", "Descripcion", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ShouldDeleteTicket()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = CreateMockTicket(ticketId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(
            ticketId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new DeleteTicketCommand(ticketId);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ticketRepositoryMock.Verify(repo => repo.DeleteAsync(
            It.IsAny<Ticket>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    //[Fact]
    //public async Task Handle_ShouldThrowException_WhenTicketNotFound()
    //{
    //    // Arrange
    //    var ticketId = Guid.NewGuid();
    //    var command = new DeleteTicketCommand(ticketId);

    //    _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(
    //        ticketId,
    //        It.IsAny<CancellationToken>()))
    //        .ReturnsAsync((Ticket)null);

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<DomainException>(() =>
    //        _handler.Handle(command, CancellationToken.None));

    //    Assert.Contains($"Ticket con ID '{ticketId}' no encontrado.", exception.Message);

    //    _ticketRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Never);

    //    _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    //}
}