using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTicketsPaged;
public class GetAllTicketsPagedQueryHandler : IRequestHandler<GetAllTicketsPagedQuery, PagedResponse<List<TicketDto>>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;

    public GetAllTicketsPagedQueryHandler(ITicketRepository ticketRepository, IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<List<TicketDto>>> Handle(
        GetAllTicketsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var (data, totalRecords) = await _ticketRepository.GetPagedTicketsAsync(
            request.PageNumber,
            request.PageSize,
            request.StatusFilter);

        return new PagedResponse<List<TicketDto>>
        {
            Data = _mapper.Map<List<TicketDto>>(data),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }
}