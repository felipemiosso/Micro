namespace Micro.API.Endpoints.HealthCheck;

public static class HealthCheckEndpoints
{
    public static IEndpointConventionBuilder MapHealthCheckEndpoints(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
    }
}
