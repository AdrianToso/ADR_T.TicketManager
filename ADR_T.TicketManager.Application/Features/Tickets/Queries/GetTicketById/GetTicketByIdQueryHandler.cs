using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    private readonly IRepository<Ticket> _ticketRepository;
    private readonly IMapper _mapper;
    public GetTicketByIdQueryHandler(IRepository<Ticket> ticketRepository, IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
    }
    public async Task<TicketDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId);
        return _mapper.Map<TicketDto>(ticket);
    }
}
