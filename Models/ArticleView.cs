namespace PeakLabs.Models;

public class ArticleView
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
    public Article Article { get; set; } = null!;
}
