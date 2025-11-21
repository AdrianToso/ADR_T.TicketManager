using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetTicketByIdQuery;
public class GetTicketByIdQueryHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetTicketByIdQueryHandler _handler;

    public GetTicketByIdQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetTicketByIdQueryHandler(_ticketRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTicketById()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket("Titulo", "Descripcion", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid());
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        var ticketDto = new TicketDto { Titulo = "Titulo", Descripcion = "Descripcion", Estado = "Abierto", Prioridad = "Alta" };
        _mapperMock.Setup(mapper => mapper.Map<TicketDto>(ticket)).Returns(ticketDto);

        var query = new ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById.GetTicketByIdQuery(ticketId); 

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(ticketDto, result);
    }
}
