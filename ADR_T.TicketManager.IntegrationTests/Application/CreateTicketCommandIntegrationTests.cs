using ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.IntegrationTests.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ADR_T.TicketManager.IntegrationTests.Application;
public sealed class CreateTicketCommandIntegrationTests : IClassFixture<TestingFixture>
{
    private readonly TestingFixture _fixture;
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;

    public CreateTicketCommandIntegrationTests(TestingFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.Context;
        _mediator = fixture.Mediator;
    }
    [Trait("Category", "Integration")]
    [Fact]
    public async Task Handle_ExistingUser_ShouldCreateTicketAndPersist()
    {
        // Resetear la base de datos antes del test
        await _fixture.ResetDatabase();

        var testUserId = Guid.NewGuid();
        var domainUser = new User("test@example.com", "test@example.com", string.Empty)
        {
            Id = testUserId
        };

        await _context.DomainUsers.AddAsync(domainUser);
        await _context.SaveChangesAsync();

        var command = new CreateTicketCommand(
            Titulo: "Ticket Creado por Handler",
            Descripcion: "Test de Integración de Handler",
            Prioridad: TicketPriority.Alta,
            CreadoByUserId: testUserId
        );

        var ticketId = await _mediator.Send(command, CancellationToken.None);

        var persistedTicket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        Assert.NotNull(persistedTicket);
        Assert.Equal(command.Titulo, persistedTicket.Titulo);
        Assert.Equal(TicketStatus.Abierto, persistedTicket.Status);
        Assert.Equal(testUserId, persistedTicket.CreadoByUserId);
        Assert.NotEqual(Guid.Empty, ticketId);
    }
}
