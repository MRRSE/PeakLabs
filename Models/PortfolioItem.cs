using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Models;

public class PortfolioItem
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string ProjectImagePath { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string ProjectName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string ProjectDescription { get; set; } = string.Empty;

    [Required, MaxLength(500), Url]
    public string ProjectLink { get; set; } = string.Empty;

    public DateTimeOffset AddedAt { get; set; }
}
