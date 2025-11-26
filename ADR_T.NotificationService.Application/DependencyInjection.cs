using Microsoft.Extensions.DependencyInjection;
using ADR_T.NotificationService.Application.Consumers;

namespace ADR_T.NotificationService.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<TicketCreadoConsumer>();
        services.AddScoped<TicketAsignadoConsumer>();
        services.AddScoped<TicketActualizadoConsumer>();
        return services;
    }
}
