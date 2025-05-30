using Xunit;
using Moq;
using MediatR;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTicketsPaged;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Application.DTOs;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.Tests.Features.Tickets.Queries.GetAllTicketsPaged;

public class GetAllTicketsPagedQueryHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllTicketsPagedQueryHandler _handler;

    public GetAllTicketsPagedQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllTicketsPagedQueryHandler(_ticketRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResponse_WithCorrectDataAndMetadata()
    {
        // Arrange
        var query = new GetAllTicketsPagedQuery { PageNumber = 2, PageSize = 5, StatusFilter = TicketStatus.Abierto };
        var ticketsFromRepo = new List<Ticket>
        {
            new Ticket("T6", "D6", TicketStatus.Abierto, TicketPriority.Alta, Guid.NewGuid()),
            // ... (simular 4 tickets más para la página 2)
            new Ticket("T10", "D10", TicketStatus.Abierto, TicketPriority.Baja, Guid.NewGuid())
        };
        var totalRecordsFromRepo = 23; // Simular total
        var expectedDtos = ticketsFromRepo.Select(t => new TicketDto { Id = t.Id, Titulo = t.Titulo, Estado = t.Status.ToString() }).ToList();

        _ticketRepositoryMock.Setup(repo => repo.GetPagedTicketsAsync(query.PageNumber, query.PageSize, query.StatusFilter))
                             .ReturnsAsync((ticketsFromRepo, totalRecordsFromRepo));

        _mapperMock.Setup(m => m.Map<List<TicketDto>>(ticketsFromRepo))
                   .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDtos, result.Data);
        Assert.Equal(query.PageNumber, result.PageNumber);
        Assert.Equal(query.PageSize, result.PageSize);
        Assert.Equal(totalRecordsFromRepo, result.TotalRecords);
        Assert.Equal((int)Math.Ceiling(totalRecordsFromRepo / (double)query.PageSize), result.TotalPages);

        _ticketRepositoryMock.Verify(repo => repo.GetPagedTicketsAsync(query.PageNumber, query.PageSize, query.StatusFilter), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TicketDto>>(ticketsFromRepo), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResponse_WhenNoTicketsMatchFilter()
    {
        // Arrange
        var query = new GetAllTicketsPagedQuery { PageNumber = 1, PageSize = 10, StatusFilter = TicketStatus.Cerrado };
        var ticketsFromRepo = new List<Ticket>(); // Lista vacía
        var totalRecordsFromRepo = 0;
        var expectedDtos = new List<TicketDto>();

        _ticketRepositoryMock.Setup(repo => repo.GetPagedTicketsAsync(query.PageNumber, query.PageSize, query.StatusFilter))
                             .ReturnsAsync((ticketsFromRepo, totalRecordsFromRepo));

        _mapperMock.Setup(m => m.Map<List<TicketDto>>(ticketsFromRepo))
                   .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(query.PageNumber, result.PageNumber);
        Assert.Equal(query.PageSize, result.PageSize);
        Assert.Equal(0, result.TotalRecords);
        Assert.Equal(0, result.TotalPages);

        _ticketRepositoryMock.Verify(repo => repo.GetPagedTicketsAsync(query.PageNumber, query.PageSize, query.StatusFilter), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TicketDto>>(ticketsFromRepo), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPagesCorrectly_ForPartialLastPage()
    {
        // Arrange
        var query = new GetAllTicketsPagedQuery { PageNumber = 3, PageSize = 10 }; // Página 3 de 10
        var ticketsFromRepo = new List<Ticket> { /* 3 tickets */ }; // Simular 3 tickets en la última página
        var totalRecordsFromRepo = 23; // 2 páginas completas (20) + 3
        var expectedDtos = ticketsFromRepo.Select(t => new TicketDto { Id = t.Id }).ToList();

        _ticketRepositoryMock.Setup(repo => repo.GetPagedTicketsAsync(query.PageNumber, query.PageSize, query.StatusFilter))
                             .ReturnsAsync((ticketsFromRepo, totalRecordsFromRepo));
        _mapperMock.Setup(m => m.Map<List<TicketDto>>(ticketsFromRepo)).Returns(expectedDtos);


        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalPages); // 23 / 10 = 2.3 -> ceil(2.3) = 3
    }
}