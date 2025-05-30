using MediatR;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Application.Contracts.Identity;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser;
public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IIdentityService _identityService; 
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginUserCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var userDetails = await _identityService.FindUserByEmailAsync(request.Email);

        if (!userDetails.Succeeded)
        {
            _logger.LogWarning("Intento de login fallido para email {Email}: Usuario no encontrado.", request.Email);
            throw new DomainException("Credenciales invalidas"); 
        }

        var passwordValid = await _identityService.CheckPasswordAsync(userDetails.UserId, request.Password);

        if (!passwordValid)
        {
            _logger.LogWarning("Intento de login fallido para email {Email} (ID: {UserId}): Contraseña incorrecta.", request.Email, userDetails.UserId);
            throw new DomainException("Credenciales invalidas"); 
        }

        var userForToken = await _identityService.GetUserByIdAsync(userDetails.UserId);
        if (userForToken == null)
        {
            _logger.LogError("Error crítico en login: Usuario de dominio no encontrado para ID {UserId} después de autenticación exitosa.", userDetails.UserId);
            throw new Exception("Error interno al procesar el login.");
        }

        string token = await _jwtTokenGenerator.GenerateTokenAsync(userForToken);

        _logger.LogInformation("Login exitoso para usuario {UserId}", userDetails.UserId);
        return new LoginResponse(token, userDetails.UserId);
    }
}