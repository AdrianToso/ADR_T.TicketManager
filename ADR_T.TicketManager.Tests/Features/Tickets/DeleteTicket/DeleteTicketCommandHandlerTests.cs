using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
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

    [Fact]
    public async Task Handle_ShouldDeleteTicket()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket("Titulo", "Descripcion", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid());
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        var command = new DeleteTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ticketRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Ticket>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
    }
}