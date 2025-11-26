using Moq;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTickets;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetAllTickets;

public class GetAllTicketsQueryHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllTicketsQueryHandler _handler;

    public GetAllTicketsQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetAllTicketsQueryHandler(_mapperMock.Object, _ticketRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new Ticket(Guid.NewGuid(), "Titulo1", "Descripcion1", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid()),
            new Ticket(Guid.NewGuid(), "Titulo2", "Descripcion2", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid())
        };

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(tickets);

        var ticketDtos = new List<TicketDto>
        {
            new TicketDto { Titulo = "Titulo1", Descripcion = "Descripcion1", Estado = "Abierto", Prioridad = "Alta" },
            new TicketDto { Titulo = "Titulo2", Descripcion = "Descripcion2", Estado = "Abierto", Prioridad = "Media" }
        };

        _mapperMock.Setup(mapper => mapper.Map<List<TicketDto>>(tickets)).Returns(ticketDtos);

        var query = new GetAllTicketsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(ticketDtos, result);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<TicketDto>>(tickets), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTicketsExist()
    {
        // Arrange
        var emptyTickets = new List<Ticket>();
        var emptyDtos = new List<TicketDto>();

        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(emptyTickets);

        _mapperMock.Setup(mapper => mapper.Map<List<TicketDto>>(emptyTickets)).Returns(emptyDtos);

        var query = new GetAllTicketsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);

        _ticketRepositoryMock.Verify(repo => repo.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<TicketDto>>(emptyTickets), Times.Once);
    }
}