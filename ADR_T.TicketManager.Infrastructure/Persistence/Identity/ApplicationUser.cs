using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IApplicationUser
    {
        public User ToDomainUser()
        {
            return new User
            {
                Id = this.Id,
                UserName = this.UserName!,
                Email = this.Email!,
                PasswordHash = this.PasswordHash
            };
        }
        public static ApplicationUser FromDomainUser(User user)
        {
            return new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                EmailConfirmed = true
            };
        }
    }
}

