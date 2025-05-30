using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Interfaces;

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
        if (asignadosIds.Any()) // <-- Validar si hay IDs antes de buscar
        {
            tecnicos = await _userRepository.GetByIdsAsync(asignadosIds);
        }


        var tecnicosDict = tecnicos.ToDictionary(u => u.Id, u => u.UserName);

        // Calcular tiempos de resolución
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
            TicketsAbiertos = allTickets.Count(t => t.Status == TicketStatus.Abierto),
            TicketsEnProgreso = allTickets.Count(t => t.Status == TicketStatus.EnProgreso),
            TicketsResueltos = allTickets.Count(t => t.Status == TicketStatus.Resuelto),
            TicketsCerrados = allTickets.Count(t => t.Status == TicketStatus.Cerrado),

            TicketsPorPrioridad = allTickets
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),

            TicketsPorEstado = allTickets
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),

            TicketsPorTecnico = allTickets
                    .Where(t => t.AsignadoUserId.HasValue)
                    .GroupBy(t => t.AsignadoUserId.Value)
                    .ToDictionary(
                        // Usar TryGetValue para manejar el caso de técnicos no encontrados (aunque GetByIdsAsync debería traerlos si existen)
                        g => tecnicosDict.TryGetValue(g.Key, out var nombre) ? nombre : "Desconocido",
                        g => g.Count()
                    ),

            TiemposDeResolucion = tiemposResolucion
        };

        _logger.LogInformation("Estadísticas calculadas: {@TicketStatistics}", statistics);

        return statistics;
    }
}