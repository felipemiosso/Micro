namespace Micro.API.Data.Models;

public class CustomFieldDefinition
{
    public Guid Id { get; set; }
    public CustomFieldTargetEntity TargetEntity { get; set; }
    public CustomFieldType FieldType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsCandidateFacing { get; set; }
    public string? ValidationJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
}

public enum CustomFieldTargetEntity
{
    Requisition,
    Application_Global,
    Application_Applied,
    Application_Interview,
    Application_Offer,
    JobPosting
}

public enum CustomFieldType
{
    ShortText,
    LongText,
    Number,
    Date,
    Boolean,
    SingleChoice
}
