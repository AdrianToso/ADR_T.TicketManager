using AutoMapper;
using Xunit;
using System;
using ADR_T.TicketManager.Application.Mappings;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using System.Linq;

namespace ADR_T.TicketManager.Tests.Application.Mappings;

public class TicketProfileTests
{
    private readonly IMapper _mapper;

    public TicketProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TicketProfile>();
            cfg.CreateMap<string, TicketStatus>().ConvertUsing(s => (TicketStatus)Enum.Parse(typeof(TicketStatus), s));
           cfg.CreateMap<string, TicketPriority>().ConvertUsing(s => (TicketPriority)Enum.Parse(typeof(TicketPriority), s));
        });

        _mapper = configuration.CreateMapper();
    }

}