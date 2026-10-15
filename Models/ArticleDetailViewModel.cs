namespace PeakLabs.Models;

public class ArticleDetailViewModel
{
    public required Article Article { get; init; }
    public IReadOnlyList<Article> RelatedArticles { get; init; } = [];
}
