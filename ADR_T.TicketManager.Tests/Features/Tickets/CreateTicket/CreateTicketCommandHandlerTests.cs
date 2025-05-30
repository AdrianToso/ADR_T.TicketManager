using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Events; // Añadido

namespace ADR_T.TicketManager.Tests.Features.Tickets.Commands.CreateTicket;
public class CreateTicketCommandHandlerUnitTests
{
    // Mocks
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<ILogger<CreateTicketCommandHandler>> _loggerMock; // Añadido
    private readonly CreateTicketCommandHandler _handler;

    public CreateTicketCommandHandlerUnitTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _loggerMock = new Mock<ILogger<CreateTicketCommandHandler>>(); // Añadido

        _unitOfWorkMock.Setup(uow => uow.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.TicketRepository).Returns(_ticketRepositoryMock.Object);

        // Pasar ILogger al constructor del Handler
        _handler = new CreateTicketCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object); // Corregido
    }

    [Fact]
    public async Task Handle_ShouldCreateTicket_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateTicketCommand("Titulo Test", "Descripcion Test", TicketPriority.Alta, userId);
        var user = new User("testuser", "test@test.com", "hash") { Id = userId };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                           .ReturnsAsync(user);
        _ticketRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Ticket>()))
                             .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Ticket>(t =>
            t.Titulo == command.Titulo &&
            t.Descripcion == command.Descripcion &&
            t.Priority == command.Prioridad &&
            t.CreadoByUserId == userId &&
            t.Status == TicketStatus.Abierto && // Estado inicial esperado
            t.DomainEvents.OfType<TicketCreadoEvent>().Any() // Verificar que el evento fue añadido
        )), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateTicketCommand("Titulo Test", "Descripcion Test", TicketPriority.Alta, userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                           .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"El usuario con ID '{userId}' no existe.", exception.Message);

        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Ticket>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
    }
}