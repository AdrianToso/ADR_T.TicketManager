using AutoMapper;
using MediatR;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetTicketById;
public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository, IMapper mapper)
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
