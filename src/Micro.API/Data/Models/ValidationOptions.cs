namespace Micro.API.Data.Models;

public record ValidationOptions
{
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }
    public DateOnly? MinDate { get; init; }
    public DateOnly? MaxDate { get; init; }
    public List<string>? Presets { get; init; }
    public string? FormatMask { get; init; }
    public List<string>? Choices { get; init; }
}
