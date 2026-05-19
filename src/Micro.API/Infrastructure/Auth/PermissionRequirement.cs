using Microsoft.AspNetCore.Authorization;

namespace Micro.API.Infrastructure.Auth;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public string Action { get; }

    public PermissionRequirement(string resource, string action)
    {
        Resource = resource;
        Action = action;
    }

    public string Permission => $"{Resource}:{Action}";
}
