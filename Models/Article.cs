using PeakLabs.Models.Identity;

namespace PeakLabs.Models;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string CoverImage { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public ArticleCategory Category { get; set; } = null!;
    public string? AuthorId { get; set; }
    public PeakLabsUser? Author { get; set; }
    public string Status { get; set; } = "published";
    public DateTimeOffset PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public ICollection<ArticleSection> Sections { get; set; } = new List<ArticleSection>();
}
