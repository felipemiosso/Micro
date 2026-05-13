using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Micro.API.Endpoints;

public static class RequisitionEndpoints
{
    public static void MapRequisitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requisitions").RequireAuthorization();

        group.MapGet("/", GetRequisitions);
        group.MapGet("/{id:guid}", GetRequisition);
        group.MapPost("/", CreateRequisition);
        group.MapPut("/{id:guid}", UpdateRequisition);
        group.MapPost("/{id:guid}/finalize", FinalizeRequisition);
        group.MapPost("/{id:guid}/close", CloseRequisition);
    }

    private static async Task<IResult> GetRequisitions(MicroDbContext db)
    {
        var requisitions = await db.Requisitions.ToListAsync();
        return Results.Ok(requisitions);
    }

    private static async Task<IResult> GetRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        return requisition is null ? Results.NotFound() : Results.Ok(requisition);
    }

    private static async Task<IResult> CreateRequisition(CreateRequisitionRequest request, MicroDbContext db, ClaimsPrincipal user)
    {
        var requisition = new Requisition
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Department = request.Department,
            Openings = request.Openings,
            Status = RequisitionStatus.Draft,
            CreatedBy = user.Identity?.Name ?? "Unknown",
            CreatedAt = DateTime.UtcNow
        };

        db.Requisitions.Add(requisition);
        await db.SaveChangesAsync();

        return Results.Created($"/api/requisitions/{requisition.Id}", requisition);
    }

    private static async Task<IResult> UpdateRequisition(Guid id, UpdateRequisitionRequest request, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition == null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be edited.");

        requisition.Title = request.Title;
        requisition.Department = request.Department;
        requisition.Openings = request.Openings;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> FinalizeRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition == null) return Results.NotFound();
        if (requisition.Status != RequisitionStatus.Draft) return Results.BadRequest("Only draft requisitions can be finalized.");

        requisition.Status = RequisitionStatus.Finalized;
        requisition.FinalizedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> CloseRequisition(Guid id, MicroDbContext db)
    {
        var requisition = await db.Requisitions.FindAsync(id);
        if (requisition == null) return Results.NotFound();
        
        requisition.Status = RequisitionStatus.Closed;
        requisition.ClosedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    public record CreateRequisitionRequest(string Title, string Department, int Openings);
    public record UpdateRequisitionRequest(string Title, string Department, int Openings);
}
