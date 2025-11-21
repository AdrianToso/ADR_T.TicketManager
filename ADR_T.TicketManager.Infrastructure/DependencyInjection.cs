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
namespace ADR_T.TicketManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["CONNECTION_STRING_TICKET"]
                               ?? configuration.GetConnectionString("DefaultConnection"); 


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

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<AppDbContext>());

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

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        // Para ICurrentUserService, necesita IHttpContextAccessor
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEventBus, RabbitMQEventBus>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "rabbitmq";
                var rabbitMqUser = configuration["RABBITMQ_USER"] ?? "guest";
                var rabbitMqPass = configuration["RABBITMQ_PASS"] ?? "guest";
                var rabbitMqVHost = configuration["RABBITMQ_VIRTUAL_HOST"] ?? configuration.GetSection("RabbitMQ")["VirtualHost"] ?? "/";

                cfg.Host(rabbitMqHost, rabbitMqVHost, h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                // Registro de mensajes de eventos
                cfg.Message<TicketAsignadoEvent>(m => m.SetEntityName("ticket_asignado_event"));
                cfg.Message<TicketCreadoEvent>(m => m.SetEntityName("ticket_creado_event"));
                cfg.Message<TicketActualizadoEvent>(m => m.SetEntityName("ticket_actualizado_event"));
            });
        });
        return services;
    }
}