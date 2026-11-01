namespace PeakLabs.Models;

public sealed class SiteVisit
{
    public long Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public DateTimeOffset VisitedAt { get; set; }
}
