using ADR_T.TicketManager.Infrastructure;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using MassTransit;
using ADR_T.TicketManager.Core.Domain.Events;

namespace ADR_T.TicketManager.IntegrationTests.Infrastructure;

public sealed class TestingFixture : IAsyncLifetime
{
    public AppDbContext Context { get; private set; }
    public IMediator Mediator { get; private set; }

    private readonly TestingDatabase _db = new();
    private readonly RabbitMqFixture _rabbit = new();
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        await _rabbit.InitializeAsync();

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(_db.ConnectionString)); // Sin EnableRetryOnFailure

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(ADR_T.TicketManager.Application.DependencyInjection).Assembly
            ));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CONNECTION_STRING_TICKET"] = _db.ConnectionString,
                ["RabbitMQ:Host"] = "localhost",
                ["RabbitMQ:Port"] = _rabbit.Port.ToString(),
                ["RabbitMQ:Username"] = "guest",
                ["RabbitMQ:Password"] = "guest",
                ["RabbitMQ:VirtualHost"] = "/"
            })
            .Build();

        services.AddInfrastructure(configuration);

        // Anular MassTransit para pruebas
        services.RemoveAll<IBus>();
        services.RemoveAll<IBusControl>();
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var uri = new Uri($"rabbitmq://{_rabbit.Hostname}:{_rabbit.Port}/{_rabbit.VirtualHost}");
                cfg.Host(uri, h =>
                {
                    h.Username(_rabbit.Username);
                    h.Password(_rabbit.Password);
                });
                cfg.Message<TicketAsignadoEvent>(m => m.SetEntityName("ticket_asignado_event"));
                cfg.Message<TicketCreadoEvent>(m => m.SetEntityName("ticket_creado_event"));
                cfg.Message<TicketActualizadoEvent>(m => m.SetEntityName("ticket_actualizado_event"));
            });
        });

        _provider = services.BuildServiceProvider();

        // Crear contexto 
        var scope = _provider.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await Context.Database.MigrateAsync();
        Mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    // Método para resetear el contexto entre tests
    public async Task ResetDatabase()
    {
        // Eliminar todos los datos sin usar transacciones
        Context.Tickets.RemoveRange(Context.Tickets);
        Context.DomainUsers.RemoveRange(Context.DomainUsers);

        await Context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context?.Dispose();
        await _db.DisposeAsync();
        _provider?.Dispose();
    }
}