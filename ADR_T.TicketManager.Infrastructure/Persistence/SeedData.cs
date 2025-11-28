using Microsoft.AspNetCore.Identity;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Enums;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ADR_T.TicketManager.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AppDbContext dbContext,
        IConfiguration configuration)
    {

        var defaultPassword = configuration["SEEDDATA__DEFAULTPASSWORD"]
                               ?? configuration["SeedData:DefaultPassword"]
                               ?? "Password123*";

        var adminPassword = configuration["SEEDDATA__ADMINPASSWORD"]
                            ?? configuration["SeedData:AdminPassword"]
                            ?? defaultPassword;

        // Seed Roles
        string[] roles = { "Admin", "Tecnico", "Usuario" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // Seed Usuarios 

        // Lista de usuarios { Email, Rol, Contraseña }
        var usersToSeed = new List<(string Email, string Role, string Password)>
        {
            ("admin@example.com", "Admin", adminPassword),
            ("tecnico1@example.com", "Tecnico", defaultPassword),
            ("tecnico2@example.com", "Tecnico", defaultPassword),
            ("tecnico3@example.com", "Tecnico", defaultPassword),
            ("usuario1@example.com", "Usuario", defaultPassword),
            ("usuario2@example.com", "Usuario", defaultPassword)
        };

        var allUsers = new List<ApplicationUser>();

        foreach (var (email, role, password) in usersToSeed)
        {
            var appUser = await EnsureUserAndRoleCreatedAsync(userManager, role, email, password);
            if (appUser != null)
            {
                allUsers.Add(appUser);
            }
        }

        foreach (var appUser in allUsers)
        {
            if (!await dbContext.Set<User>().AnyAsync(u => u.Id == appUser.Id))
            {
                var domainUser = new User(appUser.UserName, appUser.Email, appUser.PasswordHash)
                {
                    Id = appUser.Id
                };
                dbContext.Set<User>().Add(domainUser);
            }
        }

        await dbContext.SaveChangesAsync();

        // Seed Tickets 
        if (!await dbContext.Set<Ticket>().AnyAsync())
        {
            var userIds = allUsers.Select(u => u.Id).ToList();
            var random = new Random();
            var tickets = new List<Ticket>();

            for (int i = 1; i <= 25; i++)
            {
                var creadoByUserId = userIds[i % userIds.Count];

                // Rotar Status y Priority para tener variedad
                var status = (TicketStatus)(i % 4 + 1); // 1: Abierto, 2: EnProgreso, 3: Resuelto, 4: Cerrado
                var priority = (TicketPriority)(i % 4 + 1); // 1: Baja, 2: Media, 3: Alta, 4: Critica

                // Asignación simple: Asignar tickets abiertos o en proceso al primer técnico.
                Guid? asignadoUserId = null;
                if (status == TicketStatus.Abierto || status == TicketStatus.EnProgreso)
                {
                    asignadoUserId = allUsers.FirstOrDefault(u => u.Email == "tecnico1@example.com")?.Id;
                }

                var ticket = new Ticket(
                    titulo: $"Ticket de Prueba {i}: Problema con la API",
                    descripcion: $"Descripción del ticket {i}. Se requiere asistencia para el estado del servicio.",
                    status: status,
                    priority: priority,
                    creadoByUserId: creadoByUserId
                );

                // Asignar el ticket si corresponde
                if (asignadoUserId.HasValue)
                {
                    ticket.GetType().GetProperty("AsignadoUserId")?.SetValue(ticket, asignadoUserId.Value);
                }

                tickets.Add(ticket);
            }

            dbContext.Set<Ticket>().AddRange(tickets);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Asegura que el ApplicationUser y su rol asociado existan.
    /// </summary>
    private static async Task<ApplicationUser?> EnsureUserAndRoleCreatedAsync(
        UserManager<ApplicationUser> userManager,
        string role,
        string email,
        string password)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        if (appUser == null)
        {
            appUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(appUser, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falló la creación del usuario {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(appUser, role))
        {
            var roleResult = await userManager.AddToRoleAsync(appUser, role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(" | ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falló la asignación de rol {role} al usuario {email}: {errors}");
            }
        }

        return appUser;
    }
}