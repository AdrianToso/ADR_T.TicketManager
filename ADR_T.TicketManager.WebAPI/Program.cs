using ADR_T.TicketManager.Application;
using ADR_T.TicketManager.Infrastructure;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.WebAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using DotNetEnv;
using System.IO;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

try
{
    var envName = builder.Environment.EnvironmentName;
    var webApiDirectory = builder.Environment.ContentRootPath;
    var solutionDirectory = Directory.GetParent(webApiDirectory)?.FullName;

    if (solutionDirectory != null)
    {
        var envFileName = $".env.{envName.ToLower()}";
        var envPath = Path.Combine(solutionDirectory, envFileName);

        if (File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);

            var debugConnString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Valor de ConnectionString: {debugConnString}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Archivo de entorno cargado para '{envName}' desde: {envPath}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Advertencia: Archivo de entorno esperado '{envFileName}' no encontrado en la ruta: {envPath}.");
            Console.WriteLine("La configuración dependerá de appsettings.json o variables de entorno existentes.");
            Console.ResetColor();
        }
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error al intentar cargar el archivo .env: {ex.Message}");
    Console.ResetColor();
}

builder.Configuration.AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ticketmanager-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCoreServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

await app.RunInfrastructureSetupAsync(builder.Configuration);

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();

app.Run();


public class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum = Enum.GetNames(context.Type)
                .Select(name => new Microsoft.OpenApi.Any.OpenApiString(name))
                .ToList<Microsoft.OpenApi.Any.IOpenApiAny>();
            schema.Type = "string";
        }
    }
}