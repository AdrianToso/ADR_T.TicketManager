// src\ADR_T.TicketManager.Tests\Features\Tickets\AssignTicket\AssignTicketCommandHandlerTests.cs
using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Necesario solo para crear la inner exception simulada

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

        // Configurar el mock de UnitOfWork para devolver los mocks de repositorios
        _unitOfWorkMock.Setup(uow => uow.TicketRepository).Returns(_ticketRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.UserRepository).Returns(_userRepositoryMock.Object);


        _handler = new AssignTicketCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    // Helper para crear un mock de Ticket
    private Ticket CreateMockTicket(Guid id)
    {
        // Usar constructor real si es posible y sensible
        return new Ticket(id, "Ticket de prueba", "Descripción", Core.Domain.Enums.TicketStatus.Abierto, Core.Domain.Enums.TicketPriority.Media, Guid.NewGuid());
    }

    // Helper para crear un mock de Usuario
    private User CreateMockUser(Guid id, string name = "Usuario Prueba", string email = "test@example.com")
    {
        // Usar constructor real si es posible y sensible
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

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId)).ReturnsAsync(tecnico);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(tecnicoId, ticket.AsignadoUserId);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
        // Verificar log o evento de dominio si es relevante para el test
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenTicketNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync((Ticket)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains($"El ticket con ID '{ticketId}' no existe.", exception.Message);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
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

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId)).ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains($"El técnico con ID '{tecnicoId}' no existe.", exception.Message);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenCommitFails() // <-- Nombre ajustado para reflejar la excepción esperada
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);
        var tecnico = CreateMockUser(tecnicoId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId)).ReturnsAsync(tecnico);

        // Simular un fallo en el commit lanzando una DbUpdateException
        var simulatedDbException = new DbUpdateException("Simulated DB error on commit", (Exception)null); // Necesitamos EF Core aquí para el mock
        // Configurar el mock del CommitAsync para lanzar la *nueva* PersistenceException que envuelve la excepción simulada
        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ThrowsAsync(new PersistenceException("Ocurrió un error de persistencia al guardar los cambios.", simulatedDbException));


        // Act & Assert
        // Ahora esperamos la PersistenceException
        var exception = await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(command, CancellationToken.None));

        // Opcional: Verificar que la excepción original esté envuelta
        Assert.IsType<DbUpdateException>(exception.InnerException);
        Assert.Equal("Simulated DB error on commit", exception.InnerException.Message);


        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowGenericException_WhenUnexpectedErrorOccurs()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var tecnicoId = Guid.NewGuid();
        var asignadorUserId = Guid.NewGuid();
        var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);

        var ticket = CreateMockTicket(ticketId);
        var tecnico = CreateMockUser(tecnicoId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(tecnicoId)).ReturnsAsync(tecnico);

        // Simular una excepción inesperada que no sea DomainException o DbUpdateException
        var unexpectedError = new InvalidOperationException("Something went wrong unexpectedly.");

        // Configurar el mock del CommitAsync para lanzar la excepción inesperada
        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ThrowsAsync(unexpectedError); // La UnitOfWork real la envolverá en PersistenceException


        // Act & Assert
        // El manejador atrapará la PersistenceException lanzada por el CommitAsync mock
        var exception = await Assert.ThrowsAsync<PersistenceException>(() => // <-- Esperamos PersistenceException porque UnitOfWork la envuelve
            _handler.Handle(command, CancellationToken.None));

        // Verificar que la excepción inesperada original esté envuelta
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("Something went wrong unexpectedly.", exception.InnerException.Message);


        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
    }
}