using ADR_T.TicketManager.Application;
using ADR_T.TicketManager.Infrastructure;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.WebAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

await app.MigrateAndSeedDatabaseAsync();

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