namespace PeakLabs.Models;

public class SiteSettings
{
    public int Id { get; set; }
    public string SiteTitle { get; set; } = "PeakLabs | Software & Innovation";
    public string SiteDescription { get; set; } = "PeakLabs is a software engineering studio sharing practical writing on architecture, cloud, AI and developer craft.";
    public string? SiteUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? OgImage { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Linkedin { get; set; }
    public string? Github { get; set; }
    public string? Instagram { get; set; }
}
