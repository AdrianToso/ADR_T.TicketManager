using Moq;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
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

    private Ticket CreateMockTicket(Guid id)
    {
        return new Ticket(id, "Titulo", "Descripcion", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ShouldReturnTicketById()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = CreateMockTicket(ticketId);

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(ticket);

        var ticketDto = new TicketDto { Titulo = "Titulo", Descripcion = "Descripcion", Estado = TicketStatus.Abierto.ToString(), Prioridad = TicketPriority.Alta.ToString(), Id = ticketId };

        _mapperMock.Setup(mapper => mapper.Map<TicketDto>(ticket)).Returns(ticketDto);

        var query = new global::ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById.GetTicketByIdQuery(ticketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(ticketDto, result);

        _ticketRepositoryMock.Verify(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<TicketDto>(ticket), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTicketNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Ticket)null);

        _mapperMock.Setup(mapper => mapper.Map<TicketDto>(null)).Returns((TicketDto)null);

        var query = new global::ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById.GetTicketByIdQuery(ticketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);

        _ticketRepositoryMock.Verify(repo => repo.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()), Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<TicketDto>(null), Times.Once);
    }
}