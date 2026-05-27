using System.Reflection;

namespace Micro.API.Infrastructure.Auth;

public record AvailableAction(string Resource, string Action, string Description, string Permission);

public static class ActionDiscovery
{
    private static List<AvailableAction>? _cachedActions;

    public static List<AvailableAction> GetAvailableActions()
    {
        if (_cachedActions != null) return _cachedActions;

        _cachedActions = new List<AvailableAction>();

        // Scan all types for methods decorated with [ResourceAction]
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ResourceActionAttribute>();
                if (attr != null)
                {
                    _cachedActions.Add(new AvailableAction(
                        attr.Resource,
                        attr.Action,
                        attr.Description,
                        attr.Permission
                    ));
                }
            }
        }

        // Deduplicate by permission string — multiple endpoints can share the same
        // Resource:Action (e.g. all three Application "Edit" endpoints). Keep first seen.
        _cachedActions = _cachedActions
            .GroupBy(a => a.Permission)
            .Select(g => g.First())
            .OrderBy(a => a.Resource)
            .ThenBy(a => a.Action)
            .ToList();

        // Also scan Minimal API endpoint metadata if possible, but standard scanning of static classes works for our structure
        return _cachedActions;
    }
}
