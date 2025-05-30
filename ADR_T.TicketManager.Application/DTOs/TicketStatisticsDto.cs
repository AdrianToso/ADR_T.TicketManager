namespace ADR_T.TicketManager.Application.DTOs;
public class TicketStatisticsDto
{
    public int TotalTickets { get; set; }
    public int TicketsAbiertos { get; set; }
    public int TicketsEnProgreso { get; set; }
    public int TicketsResueltos { get; set; }
    public int TicketsCerrados { get; set; }
    public Dictionary<string, int> TicketsPorPrioridad { get; set; } = new();
    public Dictionary<string, int> TicketsPorEstado { get; set; } = new();
    public Dictionary<string, int> TicketsPorTecnico { get; set; } = new();
    public Dictionary<string, int> TiemposDeResolucion { get; set; } = new();

}