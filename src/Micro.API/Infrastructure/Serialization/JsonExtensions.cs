using System.Text.Json.Serialization;

namespace Micro.API.Infrastructure.Serialization;

public static class JsonExtensions
{
    public static void AddJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}
