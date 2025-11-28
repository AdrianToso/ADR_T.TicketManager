using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.Infrastructure.Repositories;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using ADR_T.TicketManager.Infrastructure.Identity;
using ADR_T.TicketManager.Application.Contracts.Identity;
using MassTransit;
using ADR_T.TicketManager.Core.Domain.Events;
using System;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // CONFIGURACIÓN DE BD PRINCIPAL

        var connectionString = configuration["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ERROR CRÍTICO DE CONFIGURACIÓN: La cadena de conexión 'DefaultConnection' no fue encontrada. " +
                "Verifique la clave 'ConnectionStrings__DefaultConnection' en el archivo .env.development."
            );
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString,
                // Configurar reintentos en caso de fallos transitorios de conexión
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // REGISTRO DE SERVICIOS Identity, Repositorios, RabbitMQ
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Configuraciones de Identity
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEventBus, RabbitMQEventBus>();


        // CONFIGURACIÓN DE MASS TRANSIT / RABBITMQ
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "guest";
                var rabbitMqVHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(rabbitMqHost, rabbitMqVHost, h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.Message<TicketAsignadoEvent>(m => m.SetEntityName("ticket_asignado_event"));
                cfg.Message<TicketCreadoEvent>(m => m.SetEntityName("ticket_creado_event"));
                cfg.Message<TicketActualizadoEvent>(m => m.SetEntityName("ticket_actualizado_event"));
            });
        });

        return services;
    }
}