using System;

namespace Micro.API.Data.Models;

public class JobPostingCustomField
{
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;

    public DateTime AssociatedAt { get; set; } = DateTime.UtcNow;
}
