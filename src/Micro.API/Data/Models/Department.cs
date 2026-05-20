using System.ComponentModel.DataAnnotations;

namespace Micro.API.Data.Models;

public class Department
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
