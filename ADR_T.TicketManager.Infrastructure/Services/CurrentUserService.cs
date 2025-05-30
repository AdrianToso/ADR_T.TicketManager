using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ADR_T.TicketManager.Core.Domain.Interfaces;

namespace ADR_T.TicketManager.Infrastructure.Services;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var result) ? result : Guid.Empty;

        }
    }
    public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

    public List<string> Roles => _httpContextAccessor.HttpContext?.User?
        .FindAll(ClaimTypes.Role)
        .Select(r => r.Value)
        .ToList() ?? new List<string>();

    public bool IsAdmin => Roles.Contains("Admin");
}
