using System.ComponentModel.DataAnnotations;

namespace Micro.API.Data.Models;

public class SalaryBand
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string Currency { get; set; } = "USD";
}
