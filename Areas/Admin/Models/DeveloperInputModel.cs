using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Areas.Admin.Models;

public class DeveloperInputModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string JobTitle { get; set; } = string.Empty;

    [EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [Url, StringLength(500)]
    public string? LinkedInUrl { get; set; }

    [Url, StringLength(500)]
    public string? GitHubUrl { get; set; }

    public bool IsActive { get; set; }
}
