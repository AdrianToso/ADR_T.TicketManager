using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Identity
{
    public static class ApplicationUserExtensions
    {
        public static User ToDomainUser(this ApplicationUser applicationUser)
        {
            return new User
            (
                userName : applicationUser.UserName,
                email: applicationUser.Email,
                passwordHash: applicationUser.PasswordHash
            )
            {
                Id = applicationUser.Id
            };
        }
    }
}
