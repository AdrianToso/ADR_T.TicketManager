using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using Microsoft.Extensions.Logging;

namespace ADR_T.TicketManager.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<JwtTokenGenerator> _logger;

    public JwtTokenGenerator(IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ILogger<JwtTokenGenerator> logger)
    {
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        if (user == null)
        {
            _logger.LogError("Intento de generar token para un usuario de dominio nulo.");
            throw new ArgumentNullException(nameof(user), "El usuario de dominio no puede ser nulo.");
        }

        _logger.LogInformation("Generando token para usuario de dominio ID: {UserId}, Email: {Email}", user.Id, user.Email);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Obtener roles de Identity
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (identityUser == null)
        {
            _logger.LogError("No se encontró el usuario de Identity con ID: {UserId}", user.Id);
            throw new InvalidOperationException($"No se encontró el usuario de Identity con ID: {user.Id}");
        }
        else
        {
            // Obtener roles de Identity
            var roles = await _userManager.GetRolesAsync(identityUser);
            _logger.LogInformation("Roles de Identity encontrados para {UserId}: {Roles}", user.Id, string.Join(", ", roles));
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        // Leer configuración JWT
        var secret = _configuration["JwtSettings:Secret"];
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];
        var expiryMinutes = _configuration.GetValue<double>("JwtSettings:ExpiryMinutes", 60);
        if (string.IsNullOrEmpty(secret))
        {
            _logger.LogCritical("La clave secreta JWT (JwtSettings:Secret) no está configurada.");
            throw new InvalidOperationException("Error de configuración JWT.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        _logger.LogInformation("Token generado exitosamente para usuario {UserId}", user.Id);

        return tokenString;
    }
}
