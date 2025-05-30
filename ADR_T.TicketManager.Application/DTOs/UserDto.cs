namespace ADR_T.TicketManager.Application.DTOs;
public sealed record UserDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Mail { get; set; }
}
