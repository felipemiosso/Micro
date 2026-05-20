using Micro.API.Data.Models;

namespace Micro.API.Data.Seed;

public static class LookupSeeder
{
    public static List<Department> GenerateDepartments() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "Engineering" },
        new() { Id = Guid.NewGuid(), Name = "Product" },
        new() { Id = Guid.NewGuid(), Name = "Design" },
        new() { Id = Guid.NewGuid(), Name = "Sales" },
        new() { Id = Guid.NewGuid(), Name = "Marketing" },
        new() { Id = Guid.NewGuid(), Name = "HR" }
    };

    public static List<SalaryBand> GenerateSalaryBands() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "Junior", MinAmount = 40000, MaxAmount = 60000 },
        new() { Id = Guid.NewGuid(), Name = "Mid", MinAmount = 60000, MaxAmount = 90000 },
        new() { Id = Guid.NewGuid(), Name = "Senior", MinAmount = 90000, MaxAmount = 130000 },
        new() { Id = Guid.NewGuid(), Name = "Staff", MinAmount = 130000, MaxAmount = 180000 }
    };

    public static List<CostCenter> GenerateCostCenters() => new()
    {
        new() { Id = Guid.NewGuid(), Code = "RD-100", Name = "Research & Development" },
        new() { Id = Guid.NewGuid(), Code = "SAL-200", Name = "Sales Global" },
        new() { Id = Guid.NewGuid(), Code = "CORP-300", Name = "Corporate Ops" }
    };
}
