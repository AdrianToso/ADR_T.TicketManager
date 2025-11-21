using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ADR_T.NotificationService.Infrastructure.Persistence;
using ADR_T.NotificationService.Application.Persistence;
using ADR_T.NotificationService.Application.Consumers;
using ADR_T.TicketManager.Core.Domain.Events;
using System;

namespace ADR_T.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["CONNECTION_STRING_NOTIFICATION"]
                             ?? configuration.GetConnectionString("NotificationConnection")
                             ?? throw new InvalidOperationException("La cadena de conexión 'CONNECTION_STRING_NOTIFICATION' o 'NotificationConnection' es requerida.");

        services.AddDbContext<NotificationDbContext>(opt =>
            opt.UseSqlServer(connectionString));
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TicketCreadoConsumer>();
            x.AddConsumer<TicketAsignadoConsumer>();
            x.AddConsumer<TicketActualizadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? configuration.GetSection("RabbitMQ")["Host"] ?? "localhost";
                var rabbitMqUser = configuration["RABBITMQ_USER"] ?? configuration.GetSection("RabbitMQ")["Username"] ?? "guest";
                var rabbitMqPass = configuration["RABBITMQ_PASS"] ?? configuration.GetSection("RabbitMQ")["Password"] ?? "guest";
                var rabbitMqVHost = configuration["RABBITMQ_VIRTUAL_HOST"] ?? configuration.GetSection("RabbitMQ")["VirtualHost"] ?? "/";

                cfg.Host(
                    rabbitMqHost,
                    rabbitMqVHost,
                    h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
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