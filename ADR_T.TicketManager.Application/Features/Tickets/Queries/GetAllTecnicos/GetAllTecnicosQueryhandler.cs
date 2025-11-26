using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Application.Features.Tickets.Queries.GetAllTecnicos;
public class GetAllTecnicosQueryhandler : IRequestHandler<GetAllTecnicosQuery, List<UserDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetAllTecnicosQueryhandler> _logger;

    public GetAllTecnicosQueryhandler(
        IIdentityService identityService,
        ILogger<GetAllTecnicosQueryhandler> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<UserDto>> Handle(GetAllTecnicosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo lista de técnicos a través de IIdentityService.");

        try
        {
            var tecnicosDto = await _identityService.GetUsersInRoleAsync("Tecnico");

            if (tecnicosDto == null || !tecnicosDto.Any())
            {
                _logger.LogWarning("IIdentityService.GetUsersInRoleAsync(\"Tecnico\") no devolvió resultados.");
                return new List<UserDto>();
            }

            _logger.LogInformation("Se obtuvieron {Count} técnicos.", tecnicosDto.Count);
            return tecnicosDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar a IIdentityService.GetUsersInRoleAsync(\"Tecnico\").");
            throw new ApplicationException("Ocurrió un error al obtener la lista de técnicos.", ex);
        }
    }
}