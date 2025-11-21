using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using ADR_T.TicketManager.Infrastructure.Identity;
using System.Security.Claims;

namespace ADR_T.TicketManager.WebAPI.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Login
        app.MapPost("/api/auth/login", async (LoginUserCommand command, IMediator mediator) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(new { token = response.Token, userId = response.UserId });
        }).AllowAnonymous();

        // Registro
        app.MapPost("/api/auth/register", async (RegisterUserCommand command, IMediator mediator) =>
        {
            var userId = await mediator.Send(command);
            return Results.Created($"/api/users/{userId}", new { UserId = userId });
        }).AllowAnonymous();

        // Crear ticket
        app.MapPost("/api/tickets", async (CreateTicketCommand command, IMediator mediator, ILogger<WebApplication> logger) =>
        {
            var ticketId = await mediator.Send(command);
            logger.LogInformation("Ticket creado con ID: {TicketId} por el usuario {UserId}", ticketId, command.CreadoByUserId);
            return Results.Created($"/api/tickets/{ticketId}", ticketId);
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<Guid>(StatusCodes.Status201Created);


        // Obtener ticket por ID
        app.MapGet("/api/tickets/{id}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetTicketByIdQuery(id);
            var ticket = await mediator.Send(query);
            return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<TicketDto>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound);

        // Obtener todos los tickets
        app.MapGet("/api/tickets", async (IMediator mediator) =>
        {
            var query = new GetAllTicketsQuery();
            var tickets = await mediator.Send(query);
            return Results.Ok(tickets);
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<List<TicketDto>>(StatusCodes.Status200OK);

        // endpoint paginado
        app.MapGet("/api/tickets/paged", async (
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
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<PagedResponse<List<TicketDto>>>(StatusCodes.Status200OK);

        // Obtener todos los técnicos
        app.MapGet("/api/tecnicos", async (IMediator mediator) =>
        {
            var query = new GetAllTecnicosQuery();
            var tecnicos = await mediator.Send(query);
            return Results.Ok(tecnicos);
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<PagedResponse<List<TicketDto>>>(StatusCodes.Status200OK);

        // Obtener usuario por ID
        app.MapGet("/api/users/{id}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetUserByIdQuery(id);
            var user = await mediator.Send(query);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<UserDto>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound);

        // obtener estadísticas de tickets
        app.MapGet("/api/tickets/statistics", async (IMediator mediator) =>
        {
            var query = new GetTicketStatisticsQuery();
            var statistics = await mediator.Send(query);
            return Results.Ok(statistics);
        }).RequireAuthorization(Policies.TecnicoOrAdmin)
          .Produces<TicketStatisticsDto>(StatusCodes.Status200OK);



        // Actualizar ticket
        app.MapPut("/api/tickets/{id}", async (Guid id, UpdateTicketCommand command, IMediator mediator) =>
        {
            command.Id = id;
            await mediator.Send(command);
            return Results.NoContent();
        }).Produces<TicketDto>()
          .RequireAuthorization(Policies.TecnicoOrAdmin);

        // Asignar ticket
        app.MapPut("/api/tickets/{ticketId}/assign/{tecnicoId}", async (Guid ticketId, Guid tecnicoId, IMediator mediator, HttpContext httpContext) =>
        {
            var asignadorUserId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var command = new AssignTicketCommand(ticketId, tecnicoId, asignadorUserId);
            await mediator.Send(command);

            return Results.Ok();
        }).RequireAuthorization(Policies.AdminPolicy)
          .Produces(StatusCodes.Status200OK);


        // Eliminar ticket
        app.MapDelete("/api/tickets/{id}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteTicketCommand(id);
            await mediator.Send(command);
            return Results.NoContent();
        }).RequireAuthorization(Policies.AdminPolicy)
          .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}