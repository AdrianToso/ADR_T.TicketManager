using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Infrastructure.Persistence;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using Microsoft.Extensions.Logging;
using ADR_T.TicketManager.Application.Contracts.Identity;
using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using AutoMapper;

namespace ADR_T.TicketManager.Infrastructure.Identity;
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<IdentityService> _logger;
    private readonly IUserRepository _usersRepository;
    private readonly IMapper _mapper;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AppDbContext dbContext,
        ILogger<IdentityService> logger,
        IUserRepository usersRepository, 
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
        _usersRepository = usersRepository;
        _mapper = mapper; 
    }
    public async Task<List<UserDto>> GetUsersInRoleAsync(string roleName)
    {
        _logger.LogInformation("Intentando obtener usuarios para el rol: {RoleName}", roleName);

        if (_userManager == null)
        {
            _logger.LogError("UserManager no fue inyectado correctamente en IdentityService.");
            return new List<UserDto>(); 
        }
        if (_usersRepository == null)
        {
            _logger.LogError("IUserRepository no fue inyectado correctamente en IdentityService.");
            return new List<UserDto>(); 
        }
        if (_mapper == null)
        {
            _logger.LogError("IMapper no fue inyectado correctamente en IdentityService.");
            return new List<UserDto>(); 
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        if (usersInRole == null || !usersInRole.Any()) 
        {
            _logger.LogWarning("No se encontraron usuarios Identity para el rol: {RoleName}", roleName);
            return new List<UserDto>();
        }
        _logger.LogInformation("Se encontraron {Count} usuarios Identity para el rol: {RoleName}", usersInRole.Count, roleName);

        var userIds = usersInRole.Select(u => u.Id).ToList();
        var domainUsers = await _usersRepository.GetByIdsAsync(userIds);

        if (domainUsers == null)
        {
            _logger.LogWarning("IUserRepository.GetByIdsAsync devolvió null inesperadamente para los IDs: {UserIds}", string.Join(", ", userIds));
            return new List<UserDto>();
        }

        if (!domainUsers.Any())
        {
            _logger.LogWarning("No se encontraron usuarios de Dominio correspondientes a los IDs de Identity: {UserIds}", string.Join(", ", userIds));
        }
        else
        {
            _logger.LogInformation("Se encontraron {Count} usuarios de Dominio correspondientes a los IDs de Identity.", domainUsers.Count);
        }
        var usersDto = _mapper.Map<List<UserDto>>(domainUsers);

        _logger.LogInformation("Mapeo a UserDto completado. Devolviendo {Count} DTOs.", usersDto?.Count ?? 0); // Usar ?. para seguridad

        return usersDto ?? new List<UserDto>();
    }
    public async Task<UserDetailsResult> FindUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new UserDetailsResult(false);
        }
        return new UserDetailsResult(true, user.Id, user.UserName, user.Email);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<RegistrationResult> RegisterUserAsync(string email, string password, string roleName)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return new RegistrationResult(false, Errors: new List<string> { $"El email '{email}' ya está registrado." });
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            return new RegistrationResult(false, Errors: result.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrEmpty(roleName))
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                _logger.LogWarning("Intento de asignar rol de Identity '{RoleName}' no existente al usuario {UserId}.", roleName, newUser.Id);
            }
            else
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("No se pudo asignar el rol de Identity '{RoleName}' al usuario {UserId}: {Errors}",
                                     roleName, newUser.Id,
                                     string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Rol '{RoleName}' asignado correctamente al usuario {UserId}.", roleName, newUser.Id);
                }
            }
        }
        _logger.LogInformation("Usuario Identity creado con éxito para {Email} con ID {UserId}.", email, newUser.Id);
        return new RegistrationResult(true, UserId: newUser.Id);
    }

    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        _logger.LogDebug("Buscando usuario de dominio con ID: {UserId}", userId);
        var user = await _dbContext.Users 
                              .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("Usuario de dominio con ID {UserId} no encontrado en la tabla Users.", userId);
        }
        else
        {
            _logger.LogDebug("Usuario de dominio encontrado: {UserId}", userId);
        }

        return user; 
    }
}