using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Application.DTOs;

namespace ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(IIdentityService identityService, IMapper mapper, ILogger<GetUserByIdQueryHandler> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Intentando obtener usuario con ID: {UserId}", request.UserId);

            var user = await _identityService.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado.", request.UserId);
                return null; 
            }

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Usuario con ID {UserId} encontrado y mapeado a DTO.", request.UserId);

            return userDto;
        }
    }
}