using Microsoft.AspNetCore.Identity;

namespace PeakLabs.Models.Identity;

public class PeakLabsUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    public string? AuthorDescription { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset ArticlesLastViewedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset PortfolioLastViewedAt { get; set; } = DateTimeOffset.UtcNow;
}
