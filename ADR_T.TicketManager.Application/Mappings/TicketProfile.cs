using AutoMapper;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Application.Mappings;
public class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ReverseMap();
    }
}
