using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ADR_T.NotificationService.Infrastructure.Persistence;

/// <summary>
/// Fábrica para permitir a las herramientas de EF Core
/// crear instancias de NotificationDbContext en tiempo de diseño.
/// Busca appsettings.json en el directorio del proyecto para obtener la cadena de conexión.
/// </summary>
public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        
        IConfigurationRoot configuration = new ConfigurationBuilder()
           .SetBasePath(basePath)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
           .Build();

        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        var connectionString = configuration.GetConnectionString("NotificationConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No se pudo encontrar la cadena de conexión 'NotificationConnection' en appsettings.json.");
        }

        optionsBuilder.UseSqlServer(connectionString);
        
        return new NotificationDbContext(optionsBuilder.Options);
    }
}
