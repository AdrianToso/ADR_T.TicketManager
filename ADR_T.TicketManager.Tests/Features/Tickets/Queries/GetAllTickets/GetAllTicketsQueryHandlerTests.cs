using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTickets;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            new Ticket("Titulo1", "Descripcion1", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid()),
            new Ticket("Titulo2", "Descripcion2", TicketStatus.Abierto, TicketPriority.Media, Guid.NewGuid())
        };
        _ticketRepositoryMock.Setup(repo => repo.ListAllAsync()).ReturnsAsync(tickets);

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
    }
}