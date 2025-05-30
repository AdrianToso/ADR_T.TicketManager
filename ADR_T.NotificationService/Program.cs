using Microsoft.EntityFrameworkCore;
using ADR_T.NotificationService.Infrastructure.Persistence;
using ADR_T.NotificationService.Application;
using ADR_T.NotificationService.Infrastructure;
using Serilog;

// Configuraci�n de logs con Serilog
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
    logger.LogInformation("Aplicaci�n de Notificaciones construida.");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        logger.LogInformation("Swagger habilitado en modo Desarrollo.");
    }

    app.UseHttpsRedirection();

    app.MapGet("/", () => $"ADR_T Notification Service ({app.Environment.EnvironmentName}) Anda!");

    ApplyMigrations(app);

    logger.LogInformation("Iniciando la aplicaci�n de Notificaciones...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicaci�n de Notificaciones fall� al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
static void ApplyMigrations(IApplicationBuilder app) 
{
    using (var scope = app.ApplicationServices.CreateScope()) 
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        try
        {
            Console.WriteLine("Aplicando migraciones de NotificationDbContext...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migraciones aplicadas exitosamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error aplicando migraciones: {ex.Message}");
           
        }
    }
}
