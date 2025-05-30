using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;

namespace ADR_T.TicketManager.Infrastructure.Persistence;
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // --- Seed Roles de Identity ---
            string[] roles = { "Admin", "Tecnico", "Usuario" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // --- Seed Usuario Admin ---
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Crear usuario de dominio sin roles
                    var domainUser = new User(adminUser.UserName, adminUser.Email, adminUser.PasswordHash)
                    {
                        Id = adminUser.Id
                    };
                    dbContext.Users.Add(domainUser);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}