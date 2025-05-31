using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketStatistics;

public class GetTicketStatisticsQueryHandler : IRequestHandler<GetTicketStatisticsQuery, TicketStatisticsDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetTicketStatisticsQueryHandler> _logger;
    private readonly IUserRepository _userRepository;

    public GetTicketStatisticsQueryHandler(ITicketRepository ticketRepository,
                                            ILogger<GetTicketStatisticsQueryHandler> logger,
                                            IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<TicketStatisticsDto> Handle(GetTicketStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculando estadísticas de tickets.");

        IReadOnlyList<Core.Domain.Entities.Ticket> allTickets;
        try
        {
            allTickets = await _ticketRepository.ListAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los tickets para calcular estadísticas.");
            throw new Exception("Ocurrió un error al obtener los datos para las estadísticas.", ex);
        }

        if (allTickets == null)
        {
            _logger.LogWarning("No se pudieron obtener tickets para calcular estadísticas.");
            return new TicketStatisticsDto();
        }

        var asignadosIds = allTickets
                .Where(t => t.AsignadoUserId.HasValue)
                .Select(t => t.AsignadoUserId.Value)
                .Distinct()
                .ToList();

        IReadOnlyList<Core.Domain.Entities.User> tecnicos = new List<Core.Domain.Entities.User>();
        if (asignadosIds.Any())
        {
            tecnicos = await _userRepository.GetByIdsAsync(asignadosIds);
        }

        var tecnicosDict = tecnicos.ToDictionary(u => u.Id, u => u.UserName);

        // Calcular Tickets Por Prioridad - Inicializar con todos los enums y luego contar
        var ticketsPorPrioridad = Enum.GetValues(typeof(TicketPriority))
                                      .Cast<TicketPriority>()
                                      .ToDictionary(p => p.ToString(), p => 0);

        foreach (var ticket in allTickets)
        {
            if (ticketsPorPrioridad.ContainsKey(ticket.Priority.ToString()))
            {
                ticketsPorPrioridad[ticket.Priority.ToString()]++;
            }
        }

        // Calcular Tickets Por Estado - Inicializar con todos los enums y luego contar
        var ticketsPorEstado = Enum.GetValues(typeof(TicketStatus))
                                   .Cast<TicketStatus>()
                                   .ToDictionary(s => s.ToString(), s => 0);

        foreach (var ticket in allTickets)
        {
            if (ticketsPorEstado.ContainsKey(ticket.Status.ToString()))
            {
                ticketsPorEstado[ticket.Status.ToString()]++;
            }
        }

        // Calcular tiempos de resolución (esta lógica ya estaba bien)
        var tiemposResolucion = new Dictionary<string, int>();
        var ticketsResueltos = allTickets
            .Where(t => t.Status == TicketStatus.Resuelto || t.Status == TicketStatus.Cerrado);

        foreach (var ticket in ticketsResueltos)
        {
            if (!ticket.FechacActualizacion.HasValue)
                continue;

            var duracion = ticket.FechacActualizacion.Value - ticket.FechacCreacion;
            var horas = duracion.TotalHours;

            string rango = horas switch
            {
                <= 24 => "0-24 horas",
                <= 48 => "24-48 horas",
                _ => "Más de 48 horas"
            };

            if (tiemposResolucion.ContainsKey(rango))
                tiemposResolucion[rango]++;
            else
                tiemposResolucion[rango] = 1;
        }

        var statistics = new TicketStatisticsDto
        {
            TotalTickets = allTickets.Count,
            TicketsAbiertos = ticketsPorEstado.TryGetValue(TicketStatus.Abierto.ToString(), out var abiertos) ? abiertos : 0,
            TicketsEnProgreso = ticketsPorEstado.TryGetValue(TicketStatus.EnProgreso.ToString(), out var enProgreso) ? enProgreso : 0,
            TicketsResueltos = ticketsPorEstado.TryGetValue(TicketStatus.Resuelto.ToString(), out var resueltos) ? resueltos : 0,
            TicketsCerrados = ticketsPorEstado.TryGetValue(TicketStatus.Cerrado.ToString(), out var cerrados) ? cerrados : 0,

            TicketsPorPrioridad = ticketsPorPrioridad,
            TicketsPorEstado = ticketsPorEstado,

            TicketsPorTecnico = allTickets
                                    .Where(t => t.AsignadoUserId.HasValue)
                                    .GroupBy(t => t.AsignadoUserId.Value)
                                    .ToDictionary(
                                        g => tecnicosDict.TryGetValue(g.Key, out var nombre) ? nombre : "Desconocido",
                                        g => g.Count()
                                    ),

            TiemposDeResolucion = tiemposResolucion
        };

        _logger.LogInformation("Estadísticas calculadas: {@TicketStatistics}", statistics);

        return statistics;
    }
}