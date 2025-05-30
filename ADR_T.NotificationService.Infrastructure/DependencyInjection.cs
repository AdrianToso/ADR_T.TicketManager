using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ADR_T.NotificationService.Infrastructure.Persistence;
using ADR_T.NotificationService.Application.Persistence;
using ADR_T.NotificationService.Application.Consumers;
using ADR_T.TicketManager.Core.Domain.Events; 

namespace ADR_T.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("NotificationConnection")
                 ?? throw new InvalidOperationException("Falta NotificationConnection");
        services.AddDbContext<NotificationDbContext>(opt =>
            opt.UseSqlServer(cs));
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        //MassTransit, Consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TicketCreadoConsumer>();
            x.AddConsumer<TicketAsignadoConsumer>();
            x.AddConsumer<TicketActualizadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rmq = configuration.GetSection("RabbitMQ");
                cfg.Host(
                    rmq["Host"] ?? "localhost",
                    rmq["VirtualHost"] ?? "/",
                    h =>
                    {
                        h.Username(rmq["Username"] ?? "guest");
                        h.Password(rmq["Password"] ?? "guest");
                    });

                cfg.Message<TicketCreadoEvent>(m =>
                    m.SetEntityName("ticket_creado_event"));
                cfg.Message<TicketAsignadoEvent>(m =>
                    m.SetEntityName("ticket_asignado_event"));
                cfg.Message<TicketActualizadoEvent>(m =>
                    m.SetEntityName("ticket_actualizado_event"));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
