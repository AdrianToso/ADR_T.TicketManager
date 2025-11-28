using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.Contracts.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Application.Common.Models;

namespace ADR_T.TicketManager.Tests.Features.Auth.Commands.LoginUser;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILogger<LoginUserCommandHandler>> _loggerMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<LoginUserCommandHandler>>();
        _handler = new LoginUserCommandHandler(
            _identityServiceMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "password123");
        var userId = Guid.NewGuid();
        var userDetailsResult = new UserDetailsResult(true, userId, "testuser", command.Email);
        var domainUser = new User("testuser", command.Email, "hash") { Id = userId };
        var expectedToken = "valid.jwt.token";

        _identityServiceMock.Setup(s => s.FindUserByEmailAsync(command.Email))
                            .ReturnsAsync(userDetailsResult);
        _identityServiceMock.Setup(s => s.CheckPasswordAsync(userId, command.Password))
                            .ReturnsAsync(true);
        _identityServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync(domainUser);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateTokenAsync(domainUser))
                              .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(userId, result.UserId);
        _identityServiceMock.Verify(s => s.FindUserByEmailAsync(command.Email), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(userId, command.Password), Times.Once);
        _identityServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        _jwtTokenGeneratorMock.Verify(g => g.GenerateTokenAsync(It.Is<User>(u => u.Id == userId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginUserCommand("unknown@example.com", "password123");
        var userDetailsResult = new UserDetailsResult(false);

        _identityServiceMock.Setup(s => s.FindUserByEmailAsync(command.Email))
                            .ReturnsAsync(userDetailsResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Credenciales invalidas", exception.Message);
        _identityServiceMock.Verify(s => s.FindUserByEmailAsync(command.Email), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _jwtTokenGeneratorMock.Verify(g => g.GenerateTokenAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "wrongpassword");
        var userId = Guid.NewGuid();
        var userDetailsResult = new UserDetailsResult(true, userId, "testuser", command.Email);

        _identityServiceMock.Setup(s => s.FindUserByEmailAsync(command.Email))
                            .ReturnsAsync(userDetailsResult);
        _identityServiceMock.Setup(s => s.CheckPasswordAsync(userId, command.Password))
                            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Credenciales invalidas", exception.Message);
        _identityServiceMock.Verify(s => s.FindUserByEmailAsync(command.Email), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(userId, command.Password), Times.Once);
        _jwtTokenGeneratorMock.Verify(g => g.GenerateTokenAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDomainUserNotFoundAfterSuccessfulAuth()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "password123");
        var userId = Guid.NewGuid();
        var userDetailsResult = new UserDetailsResult(true, userId, "testuser", command.Email);

        _identityServiceMock.Setup(s => s.FindUserByEmailAsync(command.Email))
                            .ReturnsAsync(userDetailsResult);
        _identityServiceMock.Setup(s => s.CheckPasswordAsync(userId, command.Password))
                            .ReturnsAsync(true);
        _identityServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Error interno al procesar el login.", exception.Message);
        _identityServiceMock.Verify(s => s.FindUserByEmailAsync(command.Email), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(userId, command.Password), Times.Once);
        _identityServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        _jwtTokenGeneratorMock.Verify(g => g.GenerateTokenAsync(It.IsAny<User>()), Times.Never);
    }
}