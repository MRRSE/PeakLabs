namespace PeakLabs.Models;

public class HomeIndexViewModel
{
    public IReadOnlyList<Article> Articles { get; init; } = [];
    public IReadOnlyList<ArticleCategorySummary> Categories { get; init; } = [];
    public int CategoryPage { get; init; }
    public int CategoryPageCount { get; init; }
}

public class ArticleCategorySummary
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ArticleCount { get; init; }
}
