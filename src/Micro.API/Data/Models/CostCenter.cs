using System.ComponentModel.DataAnnotations;

namespace Micro.API.Data.Models;

public class CostCenter
{
    public Guid Id { get; set; }
    [Required]
    public string Code { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;
}
