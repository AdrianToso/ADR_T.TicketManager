using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTickets;
public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, List<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    public GetAllTicketsQueryHandler(IMapper mapper, ITicketRepository ticketRepository)
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
