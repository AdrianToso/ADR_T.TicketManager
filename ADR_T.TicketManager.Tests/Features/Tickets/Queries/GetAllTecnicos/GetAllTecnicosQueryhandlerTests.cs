using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTecnicos;
using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetAllTecnicos;

public class GetAllTecnicosQueryhandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ILogger<GetAllTecnicosQueryhandler>> _loggerMock;
    private readonly GetAllTecnicosQueryhandler _handler;

    public GetAllTecnicosQueryhandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _loggerMock = new Mock<ILogger<GetAllTecnicosQueryhandler>>();
        _handler = new GetAllTecnicosQueryhandler(_identityServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfUserDtos_WhenTecnicosExist()
    {
        // Arrange
        var query = new GetAllTecnicosQuery();
        var expectedTecnicos = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid(), Nombre = "Tecnico Uno", Mail = "tec1@example.com" },
            new UserDto { Id = Guid.NewGuid(), Nombre = "Tecnico Dos", Mail = "tec2@example.com" }
        };

        _identityServiceMock.Setup(s => s.GetUsersInRoleAsync("Tecnico"))
                            .ReturnsAsync(expectedTecnicos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTecnicos.Count, result.Count);
        Assert.Equal(expectedTecnicos, result); 
        _identityServiceMock.Verify(s => s.GetUsersInRoleAsync("Tecnico"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTecnicosFound()
    {
        // Arrange
        var query = new GetAllTecnicosQuery();
        var emptyList = new List<UserDto>();

        _identityServiceMock.Setup(s => s.GetUsersInRoleAsync("Tecnico"))
                            .ReturnsAsync(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _identityServiceMock.Verify(s => s.GetUsersInRoleAsync("Tecnico"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenIdentityServiceReturnsNull()
    {
        // Arrange
        var query = new GetAllTecnicosQuery();

        _identityServiceMock.Setup(s => s.GetUsersInRoleAsync("Tecnico"))
                            .ReturnsAsync((List<UserDto>?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result); 
        Assert.Empty(result);
        _identityServiceMock.Verify(s => s.GetUsersInRoleAsync("Tecnico"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowApplicationException_WhenIdentityServiceThrows()
    {
        // Arrange
        var query = new GetAllTecnicosQuery();
        var simulatedException = new InvalidOperationException("Identity service error");

        _identityServiceMock.Setup(s => s.GetUsersInRoleAsync("Tecnico"))
                            .ThrowsAsync(simulatedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Ocurrió un error al obtener la lista de técnicos.", exception.Message);
        Assert.Equal(simulatedException, exception.InnerException); 
        _identityServiceMock.Verify(s => s.GetUsersInRoleAsync("Tecnico"), Times.Once);
    }
}