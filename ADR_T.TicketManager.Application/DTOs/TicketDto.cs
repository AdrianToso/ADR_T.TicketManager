namespace ADR_T.TicketManager.Application.DTOs;
public sealed record TicketDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Estado { get; set; }
    public string Prioridad { get; set; }
    public Guid CreadoPorUsuarioId { get; set; }
}
