using Microsoft.AspNetCore.Authorization;
public static class Policies
{
    public const string AdminPolicy = "Admin";
    public const string TecnicoPolicy = "Tecnico";
    public const string UsuarioPolicy = "Usuario";
    public const string TecnicoOrAdmin = "TecnicoOrAdmin";

    public static void ConfigurarPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(AdminPolicy, policy => policy.RequireRole("Admin"));
        options.AddPolicy(TecnicoPolicy, policy => policy.RequireRole("Tecnico"));
        options.AddPolicy(UsuarioPolicy, policy => policy.RequireRole("Usuario"));
        options.AddPolicy(TecnicoOrAdmin, policy => policy.RequireRole("Admin", "Tecnico"));
    }
}