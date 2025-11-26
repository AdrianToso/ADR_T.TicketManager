using ADR_T.TicketManager.Application.DTOs;
using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTecnicos;
public class GetAllTecnicosQuery : IRequest<List<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

