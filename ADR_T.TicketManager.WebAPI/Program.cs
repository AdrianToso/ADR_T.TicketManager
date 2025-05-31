using System.Text;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using ADR_T.TicketManager.Application;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;
using ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTecnicos;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTickets;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTicketsPaged;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
using ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketStatistics;
using ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Infrastructure;
using ADR_T.TicketManager.Infrastructure.Persistence;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Configuración de logs con Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ticketmanager-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar serialización de Enums como strings
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//  Inyección de dependencias y servicios
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.SchemaFilter<EnumSchemaFilter>();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ticket Manager API", Version = "v1" });
    //define el esquema de seguridad JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowAngular", builder =>
    {
        builder.WithOrigins("http://localhost:4200") 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
        
    });
});

// Configuración de autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
    };
});
// Configurar políticas basadas en roles Identity
builder.Services.AddAuthorization(options =>
{
    Policies.ConfigurarPolicies(options);
});
//  Configuración de Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

var app = builder.Build();

// Migraciones y seed de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

//  Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

#region POST Endpoints

// Login
app.MapPost("/api/auth/login", async (LoginUserCommand command, IMediator mediator) =>
{
    var response = await mediator.Send(command);
    return Results.Ok(new
    {
        token = response.Token,
        userId = response.UserId
    });
})
    .AllowAnonymous();

// Registro
app.MapPost("/api/auth/register", async (RegisterUserCommand command, IMediator mediator) =>
{
    var userId = await mediator.Send(command);
    return Results.Created($"/api/users/{userId}", new { UserId = userId });
})
    .AllowAnonymous();

// Crear ticket
app.MapPost("/api/tickets", async (CreateTicketCommand command, IMediator mediator, ILogger<Program> logger) =>
{
    var ticketId = await mediator.Send(command);
    logger.LogInformation("Ticket creado con ID: {TicketId} por el usuario {UserId}", ticketId, command.CreadoByUserId);
    return Results.Created($"/api/tickets/{ticketId}", ticketId);
}).RequireAuthorization(Policies.TecnicoOrAdmin)
    .Produces<Guid>(StatusCodes.Status201Created);

#endregion POST Endpoints

#region GET Endpoints

// Obtener ticket por ID
app.MapGet("/api/tickets/{id}", [Authorize(Policy = Policies.AdminPolicy)] async (Guid id, IMediator mediator) =>
{
    var query = new GetTicketByIdQuery(id);
    var ticket = await mediator.Send(query);
    return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
})
    .RequireAuthorization(Policies.TecnicoOrAdmin)
    .Produces<TicketDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound); ;

// Obtener todos los tickets
app.MapGet("/api/tickets", [Authorize(Policy = Policies.TecnicoOrAdmin)] async (IMediator mediator) =>
{
    var query = new GetAllTicketsQuery();
    var tickets = await mediator.Send(query);
    return Results.Ok(tickets);
}).RequireAuthorization(Policies.TecnicoOrAdmin)
  .Produces<List<TicketDto>>(StatusCodes.Status200OK);

//  endpoint paginado
app.MapGet("/api/tickets/paged", [Authorize(Policy = Policies.AdminPolicy)] async (
    IMediator mediator,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] TicketStatus? statusFilter = null) =>
{
    var query = new GetAllTicketsPagedQuery
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
        StatusFilter = statusFilter
    };
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
  .RequireAuthorization(Policies.TecnicoOrAdmin)
  .Produces<PagedResponse<List<TicketDto>>>(StatusCodes.Status200OK);

// Obtener todos los técnicos
app.MapGet("/api/tecnicos", async (IMediator mediator) =>
{
    var query = new GetAllTecnicosQuery();
    var tecnicos = await mediator.Send(query);
    return Results.Ok(tecnicos);
})
    .RequireAuthorization(Policies.TecnicoOrAdmin)
    .Produces<PagedResponse<List<TicketDto>>>(StatusCodes.Status200OK);

// Obtener usuario por ID
app.MapGet("/api/users/{id}", async (Guid id, IMediator mediator) =>
{
    var query = new GetUserByIdQuery(id);
    var user = await mediator.Send(query);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.RequireAuthorization(Policies.TecnicoOrAdmin)
.Produces<UserDto>(StatusCodes.Status200OK) 
.Produces(StatusCodes.Status404NotFound);

#endregion GET Endpoints

#region PUT Endpoints

// Actualizar ticket
app.MapPut("/api/tickets/{id}", async (Guid id, UpdateTicketCommand command, IMediator mediator) =>
{
    command.Id = id;
    await mediator.Send(command);
    return Results.NoContent();
}).Produces<TicketDto>()
  .RequireAuthorization(Policies.TecnicoOrAdmin);

#endregion PUT Endpoints

#region DELETE Endpoints

// Eliminar ticket
app.MapDelete("/api/tickets/{id}", async (Guid id, IMediator mediator) =>
{
    var command = new DeleteTicketCommand(id);
    await mediator.Send(command);
    return Results.NoContent();
})
    .RequireAuthorization(Policies.AdminPolicy)
    .Produces(StatusCodes.Status204NoContent);

#endregion DELETE Endpoints

app.MapPut("/api/tickets/{ticketId}/assign/{tecnicoId}", [Authorize(Policy = Policies.AdminPolicy)] async (Guid ticketId, Guid tecnicoId, IMediator mediator, HttpContext httpContext) =>
{
    var asignadorUserId = Guid.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

    var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);
    await mediator.Send(command);

    return Results.Ok();
})
.RequireAuthorization(Policies.AdminPolicy)
.Produces(StatusCodes.Status200OK);

// obtener estadísticas de tickets
app.MapGet("/api/tickets/statistics", [Authorize(Policy = Policies.TecnicoOrAdmin)] async (IMediator mediator) =>
{
    var query = new GetTicketStatisticsQuery();
    var statistics = await mediator.Send(query);
    return Results.Ok(statistics);
})
.RequireAuthorization(Policies.TecnicoOrAdmin)
.Produces<TicketStatisticsDto>(StatusCodes.Status200OK);

app.Run();

// Clase auxiliar para Swagger
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum = Enum.GetNames(context.Type)
                .Select(name => new OpenApiString(name))
                .ToList<IOpenApiAny>();
            schema.Type = "string";
        }
    }
}