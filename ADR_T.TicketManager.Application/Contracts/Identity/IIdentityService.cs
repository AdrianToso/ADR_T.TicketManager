﻿using ADR_T.TicketManager.Application.DTOs;
using ADR_T.TicketManager.Core.Domain.Entities; 

namespace ADR_T.TicketManager.Application.Contracts.Identity;
public record AuthenticationResult(bool Succeeded, Guid UserId = default, string Token = null, IEnumerable<string> Errors = null);
public record RegistrationResult(bool Succeeded, Guid UserId = default, IEnumerable<string> Errors = null);
public record UserDetailsResult(bool Succeeded, Guid UserId = default, string UserName = null, string Email = null); 
public interface IIdentityService
{
    Task<UserDetailsResult> FindUserByEmailAsync(string email);

    Task<bool> CheckPasswordAsync(Guid userId, string password);

    Task<RegistrationResult> RegisterUserAsync(string email, string password, string roleName);

    Task<User> GetUserByIdAsync(Guid userId);
    Task<List<UserDto>> GetUsersInRoleAsync(string roleName);
}