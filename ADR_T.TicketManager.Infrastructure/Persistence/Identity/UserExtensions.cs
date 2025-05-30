using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Identity
{
    public static class UserExtensions
    {
        public static User ToDomainUser(this ApplicationUser applicationUser)
        {
            return new User(applicationUser.UserName, applicationUser.Email, applicationUser.PasswordHash)
            {
                Id = applicationUser.Id
            };
        }
    }
}
