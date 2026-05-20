using Micro.API.Data;
using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization("Admin");

        // Departments
        group.MapGet("/departments", GetDepartments);
        group.MapPost("/departments", CreateDepartment);
        group.MapPut("/departments/{id:guid}", UpdateDepartment);

        // Salary Bands
        group.MapGet("/salary-bands", GetSalaryBands);
        group.MapPost("/salary-bands", CreateSalaryBand);
        group.MapPut("/salary-bands/{id:guid}", UpdateSalaryBand);

        // Cost Centers
        group.MapGet("/cost-centers", GetCostCenters);
        group.MapPost("/cost-centers", CreateCostCenter);
        group.MapPut("/cost-centers/{id:guid}", UpdateCostCenter);
    }

    // Departments
    private static async Task<IResult> GetDepartments(MicroDbContext db) => 
        Results.Ok(await db.Departments.OrderBy(d => d.Name).ToListAsync());

    private static async Task<IResult> CreateDepartment(Department department, MicroDbContext db)
    {
        department.Id = Guid.NewGuid();
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        return Results.Created($"/api/admin/departments/{department.Id}", department);
    }

    private static async Task<IResult> UpdateDepartment(Guid id, Department input, MicroDbContext db)
    {
        var dept = await db.Departments.FindAsync(id);
        if (dept is null) return Results.NotFound();
        dept.Name = input.Name;
        dept.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // Salary Bands
    private static async Task<IResult> GetSalaryBands(MicroDbContext db) => 
        Results.Ok(await db.SalaryBands.OrderBy(s => s.Name).ToListAsync());

    private static async Task<IResult> CreateSalaryBand(SalaryBand band, MicroDbContext db)
    {
        band.Id = Guid.NewGuid();
        db.SalaryBands.Add(band);
        await db.SaveChangesAsync();
        return Results.Created($"/api/admin/salary-bands/{band.Id}", band);
    }

    private static async Task<IResult> UpdateSalaryBand(Guid id, SalaryBand input, MicroDbContext db)
    {
        var band = await db.SalaryBands.FindAsync(id);
        if (band is null) return Results.NotFound();
        band.Name = input.Name;
        band.MinAmount = input.MinAmount;
        band.MaxAmount = input.MaxAmount;
        band.Currency = input.Currency;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // Cost Centers
    private static async Task<IResult> GetCostCenters(MicroDbContext db) => 
        Results.Ok(await db.CostCenters.OrderBy(c => c.Code).ToListAsync());

    private static async Task<IResult> CreateCostCenter(CostCenter costCenter, MicroDbContext db)
    {
        costCenter.Id = Guid.NewGuid();
        db.CostCenters.Add(costCenter);
        await db.SaveChangesAsync();
        return Results.Created($"/api/admin/cost-centers/{costCenter.Id}", costCenter);
    }

    private static async Task<IResult> UpdateCostCenter(Guid id, CostCenter input, MicroDbContext db)
    {
        var cc = await db.CostCenters.FindAsync(id);
        if (cc is null) return Results.NotFound();
        cc.Code = input.Code;
        cc.Name = input.Name;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
