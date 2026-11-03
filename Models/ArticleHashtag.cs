namespace PeakLabs.Models;

public class ArticleHashtag
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public string Tag { get; set; } = string.Empty;
}
