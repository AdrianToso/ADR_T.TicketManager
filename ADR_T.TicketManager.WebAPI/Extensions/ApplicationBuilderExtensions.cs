using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;

namespace ADR_T.TicketManager.WebAPI.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task RunInfrastructureSetupAsync(this IApplicationBuilder app, IConfiguration configuration)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();

        var dbContext = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var env = services.GetRequiredService<IHostEnvironment>();

        try
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("Aplicando migraciones automáticamente en entorno de desarrollo...");
                await dbContext.Database.MigrateAsync();
            }

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                var pendingList = string.Join(", ", pendingMigrations);
                logger.LogCritical("ERROR CRÍTICO: Migraciones pendientes detectadas: {PendingMigrations}. La aplicación se detiene para garantizar la integridad del esquema.", pendingList);
                Environment.Exit(1);
            }
            logger.LogInformation("Verificación de esquema completada. No hay migraciones pendientes.");

            if (env.IsDevelopment())
            {
                logger.LogInformation("Ejecutando seeding de datos (Developer Setup).");
                await SeedData.InitializeAsync(userManager, roleManager, dbContext, configuration);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "ERROR FATAL: Fallo irrecuperable en el setup de infraestructura.");
            Environment.Exit(1);
        }
    }
}