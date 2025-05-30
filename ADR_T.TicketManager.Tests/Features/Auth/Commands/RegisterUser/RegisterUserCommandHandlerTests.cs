using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.Contracts.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ADR_T.TicketManager.Tests.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock; // Mock para el repo dentro de UoW
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();

        // Configurar UoW para devolver el mock del repositorio
        _unitOfWorkMock.Setup(uow => uow.UserRepository).Returns(_userRepositoryMock.Object);

        _handler = new RegisterUserCommandHandler(
            _identityServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserId_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var command = new RegisterUserCommand("newuser@example.com", "password123");
        var expectedUserId = Guid.NewGuid();
        var registrationResult = new RegistrationResult(true, expectedUserId);

        _identityServiceMock.Setup(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()))
                            .ReturnsAsync(registrationResult);

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                           .Returns(Task.CompletedTask); // Simular AddAsync

        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ReturnsAsync(1); // Simular Commit exitoso

        // Act
        var resultUserId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedUserId, resultUserId);
        _identityServiceMock.Verify(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            u.Id == expectedUserId &&
            u.Email == command.Email &&
            u.UserName == command.Email // Asumiendo que UserName se inicializa con Email
            )), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenIdentityRegistrationFails()
    {
        // Arrange
        var command = new RegisterUserCommand("fail@example.com", "password123");
        var errors = new List<string> { "Identity error 1", "Identity error 2" };
        var registrationResult = new RegistrationResult(false, Errors: errors);

        _identityServiceMock.Setup(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()))
                            .ReturnsAsync(registrationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("No se pudo registrar el usuario.", exception.Message);
        Assert.Contains("Identity error 1", exception.Message);
        Assert.Contains("Identity error 2", exception.Message);
        _identityServiceMock.Verify(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenIdentityReturnsSuccessButNoUserId()
    {
        // Arrange
        var command = new RegisterUserCommand("nouserid@example.com", "password123");
        // Succeeded=true, pero UserId es default (Guid.Empty)
        var registrationResult = new RegistrationResult(true, Guid.Empty);

        _identityServiceMock.Setup(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()))
                            .ReturnsAsync(registrationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Error inesperado durante el registro.", exception.Message);
        _identityServiceMock.Verify(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCommitFails()
    {
        // Arrange
        var command = new RegisterUserCommand("commitfail@example.com", "password123");
        var expectedUserId = Guid.NewGuid();
        var registrationResult = new RegistrationResult(true, expectedUserId);

        _identityServiceMock.Setup(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()))
                            .ReturnsAsync(registrationResult);

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.CommitAsync(CancellationToken.None))
                       .ThrowsAsync(new DbUpdateException("Simulated DB error")); // Simular error en Commit

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => // Espera la excepción específica de la BD
            _handler.Handle(command, CancellationToken.None));

        _identityServiceMock.Verify(s => s.RegisterUserAsync(command.Email, command.Password, UserRoleType.Usuario.ToString()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(CancellationToken.None), Times.Once); // Commit se intentó
    }
}