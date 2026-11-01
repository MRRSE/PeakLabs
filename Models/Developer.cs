namespace PeakLabs.Models;

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
