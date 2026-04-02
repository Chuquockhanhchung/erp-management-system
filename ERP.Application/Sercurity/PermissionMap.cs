using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ERP.API.Security;

public static class PermissionMap
{
    public static IEnumerable<string> ForRole(string role) => role switch
    {
        "Admin" => new[] { "users.read", "users.write", "products.read", "products.write", "inventory.read", "inventory.write", "orders.read", "orders.write", "orders.approve", "invoices.read", "payments.write", "audit.read" },
        "Staff" => new[] { "products.read", "products.write", "inventory.read", "inventory.write", "orders.read", "orders.write", "invoices.read", "payments.write" },
        _ => new[] { "orders.read.self", "orders.write.self", "invoices.read.self" }
    };
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var perms = context.User.FindAll("perm").Select(c => c.Value);
        if (perms.Contains(requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("PERM:", StringComparison.OrdinalIgnoreCase))
        {
            var perm = policyName["PERM:".Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(perm))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return base.GetPolicyAsync(policyName);
    }
}