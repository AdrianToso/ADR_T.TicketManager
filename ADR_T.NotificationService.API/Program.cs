using Microsoft.EntityFrameworkCore;
using ADR_T.NotificationService.Infrastructure.Persistence;
using ADR_T.NotificationService.Application;
using ADR_T.NotificationService.Infrastructure;
using Serilog;

// Configuración de logs con Serilog
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File("logs/notificationservice-.log", rollingInterval: RollingInterval.Day)
  .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);


    builder.Host.UseSerilog((context, service, configuration) => configuration
      .ReadFrom.Configuration(context.Configuration)
      .ReadFrom.Services(service)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .WriteTo.File("logs/notificationservice-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

    Log.Information("Configurando el servicio de notificaciones...");

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Aplicación de Notificaciones construida.");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        logger.LogInformation("Swagger habilitado en modo Desarrollo.");
    }

    //   app.UseHttpsRedirection();

    app.MapGet("/", () => $"ADR_T Notification Service ({app.Environment.EnvironmentName}) Anda!");

    await ApplyMigrationsAsync(app, (Microsoft.Extensions.Logging.ILogger)logger);

    logger.LogInformation("Iniciando la aplicación de Notificaciones...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación de Notificaciones falló al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyMigrationsAsync(WebApplication app, Microsoft.Extensions.Logging.ILogger logger)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        try
        {
            logger.LogInformation("Aplicando migraciones de NotificationDbContext...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas exitosamente para NotificationDbContext.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ERROR CRÍTICO: Error aplicando migraciones para NotificationDbContext.");
        }
    }
}