namespace Micro.API.Endpoints;

public static class HealthCheckEndpoints
{
    public static void MapHealthCheckEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", HandleHealthCheck)
           .WithName("HealthCheck");
    }

    private static IResult HandleHealthCheck()
    {
        return Results.Ok(new { status = "Healthy" });
    }
}
