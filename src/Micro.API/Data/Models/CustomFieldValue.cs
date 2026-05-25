namespace Micro.API.Data.Models;

public class CustomFieldValue
{
    public Guid Id { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition Definition { get; set; } = null!;
    public Guid EntityId { get; set; }
    public CustomFieldTargetEntity TargetEntity { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
