namespace PeakLabs.Models;

public class ArticleIndexViewModel
{
    public IReadOnlyList<Article> Articles { get; init; } = [];
    public IReadOnlyList<ArticleCategory> Categories { get; init; } = [];
}
