using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Linq;
using Xunit;

namespace ADR_T.TicketManager.Tests.Core.Domain.Constants;

public class PoliciesTests
{
    [Fact]
    public void ConfigurarPolicies_ShouldRegisterAllPoliciesWithCorrectRoles()
    {
        // ARRANGE
        var options = new AuthorizationOptions();

        // ACT
        Policies.ConfigurarPolicies(options);

        // ASSERT

        void AssertPolicyHasRoles(string policyName, params string[] requiredRoles)
        {
            var policy = options.GetPolicy(policyName);

            Assert.NotNull(policy);

            var roleRequirement = policy.Requirements
                .OfType<RolesAuthorizationRequirement>()
                .FirstOrDefault();

            Assert.NotNull(roleRequirement);

            var expectedRolesSet = new HashSet<string>(requiredRoles);
            var actualRolesSet = new HashSet<string>(roleRequirement.AllowedRoles);

            Assert.True(expectedRolesSet.SetEquals(actualRolesSet),
                $"La política '{policyName}' no contiene los roles requeridos: {string.Join(", ", requiredRoles)}.");
        }

        AssertPolicyHasRoles(Policies.AdminPolicy, "Admin");

        AssertPolicyHasRoles(Policies.TecnicoPolicy, "Tecnico");

        AssertPolicyHasRoles(Policies.UsuarioPolicy, "Usuario");

        AssertPolicyHasRoles(Policies.TecnicoOrAdmin, "Admin", "Tecnico");
    }
}