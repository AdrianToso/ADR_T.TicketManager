using Moq;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.AssignTicket;

public class AssignTicketCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<AssignTicketCommandHandler>> _loggerMock;
    private readonly AssignTicketCommandHandler _handler;

    public AssignTicketCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AssignTicketCommandHandler>>();

        _unitOfWorkMock.Setup(uow => uow.Tickets).Returns(_ticketRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.Users).Returns(_userRepositoryMock.Object);

        _handler = new AssignTicketCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    private Ticket CreateMockTicket(Guid id)
    {
        return new Ticket(id, "Ticket de prueba", "Descripción", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid());
    }

    private User CreateMockUser(Guid id, string name = "Usuario Prueba", string email = "test@example.com")
    {
        return new User(name, email, "hash") { Id = id };
    }


    [Fact]
    public async Task Handle_ShouldAssignTicketSuccessfully()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);
        var tecnico = CreateMockUser(tecnicoId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(tecnicoId, ticket.AsignadoUserId);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenTicketNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>())).ReturnsAsync((Ticket)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains($"El ticket con ID '{ticketId}' no existe.", exception.Message);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenTecnicoNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId, It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains($"El técnico con ID '{tecnicoId}' no existe.", exception.Message);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenCommitFails()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);
        var tecnico = CreateMockUser(tecnicoId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);

        var simulatedDbException = new DbUpdateException("Simulated DB error on commit", (Exception)null);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new PersistenceException("Ocurrió un error de persistencia al guardar los cambios.", simulatedDbException));


        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.IsType<DbUpdateException>(exception.InnerException);
        Assert.Equal("Simulated DB error on commit", exception.InnerException.Message);


        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUnexpectedErrorOccurs()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);
        var tecnico = CreateMockUser(tecnicoId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var unexpectedError = new InvalidOperationException("Simulated unexpected internal operation error.");

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(unexpectedError);


        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Simulated unexpected internal operation error.", exception.Message);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}