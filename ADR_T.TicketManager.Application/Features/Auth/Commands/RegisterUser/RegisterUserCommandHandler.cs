using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Core.Domain.Exceptions;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var registrationResult = await _identityService.RegisterUserAsync(request.Email, request.Password, UserRoleType.Usuario.ToString());

        if (!registrationResult.Succeeded)
        {
            _logger.LogError("Error al registrar usuario Identity para {Email}: {Errors}", request.Email, string.Join(", ", registrationResult.Errors ?? new List<string>()));
            throw new DomainException($"No se pudo registrar el usuario. Errores: {string.Join(", ", registrationResult.Errors ?? new List<string>())}");
        }

        if (registrationResult.UserId == Guid.Empty)
        {
            _logger.LogError("Registro de Identity exitoso para {Email} pero no se devolvió UserId.", request.Email);
            throw new InvalidOperationException("Error inesperado durante el registro.");
        }

        _logger.LogInformation("Usuario Identity creado exitosamente para {Email} con ID {UserId}", request.Email, registrationResult.UserId);

        var domainUser = new User(request.Email, request.Email, string.Empty)
        {
            Id = registrationResult.UserId
        };


        await _unitOfWork.Users.AddAsync(domainUser);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Usuario de dominio creado y rol asignado para {UserId}", domainUser.Id);

        return domainUser.Id;
    }
}