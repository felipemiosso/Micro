using System;

namespace Micro.API.Data.Models;

public class RequisitionCustomField
{
    public Guid RequisitionId { get; set; }
    public Requisition Requisition { get; set; } = null!;

    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;

    public DateTime AssociatedAt { get; set; } = DateTime.UtcNow;
}
