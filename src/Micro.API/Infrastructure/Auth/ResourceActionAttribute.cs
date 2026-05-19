namespace Micro.API.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Method)]
public class ResourceActionAttribute : Attribute
{
    public string Resource { get; }
    public string Action { get; }
    public string Description { get; }

    public ResourceActionAttribute(string resource, string action, string description)
    {
        Resource = resource;
        Action = action;
        Description = description;
    }

    public string Permission => $"{Resource}:{Action}";
}
