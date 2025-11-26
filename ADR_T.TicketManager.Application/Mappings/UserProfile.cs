using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;
using AutoMapper;

namespace ADR_T.TicketManager.Application.Mappings;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Mail, opt => opt.MapFrom(src => src.Email))
            .ReverseMap();
    }
}
