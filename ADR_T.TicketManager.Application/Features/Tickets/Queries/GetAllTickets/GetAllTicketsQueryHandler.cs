using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTickets;
public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, List<TicketDto>>
{
    private readonly IRepository<Ticket> _ticketRepository;
    private readonly IMapper _mapper;
    public GetAllTicketsQueryHandler(IMapper mapper, IRepository<Ticket> ticketRepository)
    {
        _mapper = mapper;
        _ticketRepository = ticketRepository;

    }
    public async Task<List<TicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.ListAllAsync();
        return _mapper.Map<List<TicketDto>>(tickets);
    }
}
