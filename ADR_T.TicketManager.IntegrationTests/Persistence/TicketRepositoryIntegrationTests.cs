using Xunit;
using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;

namespace ADR_T.TicketManager.IntegrationTests.Persistence;
public class TicketRepositoryIntegrationTests
{
    private readonly AppDbContext _context;
    private readonly TicketRepository _repository;
    private readonly Guid _creatorId = Guid.NewGuid();
    private readonly Guid _updatedByUserId = Guid.NewGuid();

    public TicketRepositoryIntegrationTests()
    {

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"IntegrationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);

        _repository = new TicketRepository(_context);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddTicket_ShouldPersistNewTicketToDatabase_AndCoverInfrastructureCode()
    {
        // ARRANGE: Crear una nueva entidad
        var newTicketId = Guid.NewGuid();
        var newTicket = new Ticket(newTicketId, "Ticket Nuevo para Add", "Desc Test Add", TicketStatus.Abierto, TicketPriority.Media, _creatorId);

        // Verificación inicial -no debe existir
        var existsBefore = await _context.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == newTicketId);
        Assert.Null(existsBefore);

        // ACT: 1. Llamar al método de adición del repositorio
        await _repository.AddAsync(newTicket);

        // ACT: 2. Ejecutar el SaveChangesAsync del DbContext para persistir la adición
        await _context.SaveChangesAsync(CancellationToken.None);

        // ASSERT: Consultar la base de datos de forma independiente para validar la creación
        // Usamos AsNoTracking para evitar conflictos con el Change Tracker.
        var persistedTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == newTicketId);

        // ASSERT: 1. La entidad debe existir
        Assert.NotNull(persistedTicket);

        // ASSERT: 2. Validación de datos creados
        Assert.Equal(newTicketId, persistedTicket.Id);
        Assert.Equal("Ticket Nuevo para Add", persistedTicket.Titulo);

        _context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task UpdateTicket_ShouldPersistChangesToDatabase_AndCoverInfrastructureCode()
    {
        var ticketId = Guid.NewGuid();
        var originalTitle = "Titulo Base";
        var newTitle = "Nuevo Titulo Compilable y Persistido";

        var originalTicket = new Ticket(ticketId, originalTitle, "Desc Base", TicketStatus.Abierto, TicketPriority.Alta, _creatorId);
        await _context.Tickets.AddAsync(originalTicket);
        await _context.SaveChangesAsync();

        // ACT: 1. Recuperar la entidad usando el Repositorio
        var ticketToUpdate = await _repository.GetByIdAsync(ticketId, CancellationToken.None);
        Assert.NotNull(ticketToUpdate);

        // ACT: 2. Actualización a través del método de dominio público
        ticketToUpdate.Update(
            titulo: newTitle, // Nuevo título
            descripcion: "Nueva Desc Compilable",
            status: TicketStatus.EnProgreso,
            priority: TicketPriority.Media,
            actualizadoPorUserId: _updatedByUserId
        );

        // ACT: 3. Ejecutar el SaveChangesAsync REAL del DbContext
        await _context.SaveChangesAsync(CancellationToken.None);

        // ASSERT: 1. Consultar la base de datos de forma independiente 
        var persistedTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);

        // ASSERT: 2. Validación final
        Assert.NotNull(persistedTicket);
        Assert.Equal(newTitle, persistedTicket.Titulo);
        Assert.Equal(TicketStatus.EnProgreso, persistedTicket.Status);

        _context.Database.EnsureDeleted();
    }
    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTicket_AndSetIsDeletedTrue()
    {
        // ARRANGE
        var ticketId = Guid.NewGuid();
        var ticketToDelete = new Ticket(ticketId, "Ticket para Soft Delete", "Desc", TicketStatus.Abierto, TicketPriority.Media, _creatorId);

        await _context.Tickets.AddAsync(ticketToDelete);
        await _context.SaveChangesAsync();

        var existsBefore = await _context.Tickets.FindAsync(ticketId);
        Assert.NotNull(existsBefore);
        Assert.False(existsBefore.IsDeleted);

        // ACT: 1. Llamar a Soft Delete
        await _repository.DeleteAsync(ticketToDelete);

        // ACT: 2. Ejecutar SaveChangesAsync REAL
        await _context.SaveChangesAsync(CancellationToken.None);

        // Desvincula la entidad del Change Tracker.
        _context.Entry(ticketToDelete).State = EntityState.Detached;

        // ASSERT: Consultar la base de datos bypaseando el filtro global (IgnoreQueryFilters)
        var softDeletedTicket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        // ASSERT: 1. La entidad aún debe existir (confirmación de que no se borró físicamente)
        Assert.NotNull(softDeletedTicket);

        // ASSERT: 2. La propiedad IsDeleted debe ser verdadera (ahora debe pasar)
        Assert.True(softDeletedTicket.IsDeleted);
    }
    [Fact]
    public async Task HardDeleteAsync_ShouldPhysicallyDeleteTicketFromDatabase_AndCoverNewMethod()
    {
        // ARRANGE: Inserción de un nuevo Ticket para eliminación física
        var ticketId = Guid.NewGuid();
        var ticketToHardDelete = new Ticket(ticketId, "Ticket para Hard Delete", "Desc", TicketStatus.Abierto, TicketPriority.Media, _creatorId);

        await _context.Tickets.AddAsync(ticketToHardDelete);
        await _context.SaveChangesAsync();

        // Verificación inicial de que existe
        var existsBefore = await _context.Tickets.FindAsync(ticketId);
        Assert.NotNull(existsBefore);

        // ACT: 1. Llamar al método de eliminación FÍSICA (HardDeleteAsync)
        await _repository.HardDeleteAsync(ticketToHardDelete);

        // ACT: 2. Ejecutar SaveChangesAsync para persistir la eliminación física
        await _context.SaveChangesAsync(CancellationToken.None);

        // ASSERT: Consultar la base de datos bypaseando el filtro global (IgnoreQueryFilters)
        // La entidad DEBE ser null, confirmando la eliminación permanente.
        var deletedTicket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        // ASSERT: 1. La entidad DEBE ser null
        Assert.Null(deletedTicket);

        _context.Database.EnsureDeleted();
    }
}