using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ADR_T.TicketManager.Application.Mappings;
using FluentValidation;

namespace ADR_T.TicketManager.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TicketProfile));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
