using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Micro.API.Infrastructure.Auth;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.Contains(':'))
        {
            var parts = policyName.Split(':');
            var resource = parts[0];
            var action = parts[1];

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(resource, action));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        // If it doesn't contain ':', treat it as a role requirement
        var rolePolicy = new AuthorizationPolicyBuilder();
        rolePolicy.RequireClaim("role", policyName);
        return Task.FromResult<AuthorizationPolicy?>(rolePolicy.Build());
    }
}
