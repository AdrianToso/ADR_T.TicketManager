using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IApplicationUser
    {
        public User ToDomainUser() => new(UserName, Email, PasswordHash)
        {
            Id = Id
        };

        public static ApplicationUser FromDomainUser(User user) => new()
        
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash = user.PasswordHash
            };
        }
    }
  

