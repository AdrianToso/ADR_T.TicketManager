using AutoMapper;
using Xunit;
using ADR_T.TicketManager.Application.Mappings;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Tests.Application.Mappings;

public class UserProfileTests
{
    private readonly IMapper _mapper;

    public UserProfileTests()
    {
        // ARRANGE
        var configuration = new MapperConfiguration(cfg =>
        {
            
            cfg.AddProfile<UserProfile>();
        });

        configuration.AssertConfigurationIsValid(); 

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldMapUserToUserDtoAndBack()
    {
        // ARRANGE
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "adminuser",
            Email = "admin@test.com"
        };

        // ACT

        var userDto = _mapper.Map<UserDto>(user);

        var userAgain = _mapper.Map<User>(userDto);

        // ASSERT
        Assert.Equal(user.UserName, userDto.Nombre);
        Assert.Equal(user.Email, userDto.Mail);

        // ASSERT
        Assert.Equal(userDto.Nombre, userAgain.UserName);
        Assert.Equal(userDto.Mail, userAgain.Email);
    }
}