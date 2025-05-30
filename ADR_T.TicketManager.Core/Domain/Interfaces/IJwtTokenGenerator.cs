using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Core.Domain.Interfaces;

public interface IJwtTokenGenerator
{
   Task<string> GenerateTokenAsync(User user);
}
