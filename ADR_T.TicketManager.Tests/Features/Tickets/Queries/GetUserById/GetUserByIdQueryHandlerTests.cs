using Moq;
using ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById; // Ajustar namespace si es necesario
using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Tests.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GetUserByIdQueryHandler>>();
        _handler = new GetUserByIdQueryHandler(_identityServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var domainUser = new User("testuser", "test@example.com", "hash") { Id = userId };
        var expectedDto = new UserDto { Id = userId, Nombre = "testuser", Mail = "test@example.com" };

        _identityServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync(domainUser);
        _mapperMock.Setup(m => m.Map<UserDto>(domainUser))
                   .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto, result);
        _identityServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(domainUser), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _identityServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync((User?)null); // Usuario no encontrado

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _identityServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<User>()), Times.Never); // No se debe llamar al mapper si el usuario es null
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMapperReturnsNull() // Caso poco probable pero posible
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var domainUser = new User("testuser", "test@example.com", "hash") { Id = userId };

        _identityServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                            .ReturnsAsync(domainUser);
        // Simular que el mapper devuelve null
        _mapperMock.Setup(m => m.Map<UserDto>(domainUser))
                   .Returns((UserDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // El handler devuelve directamente lo que retorna el mapper
        Assert.Null(result);
        _identityServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(domainUser), Times.Once);
    }
}