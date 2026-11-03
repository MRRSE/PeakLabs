namespace PeakLabs.Models;

public class PortfolioIndexViewModel
{
    public IReadOnlyList<PortfolioItem> Items { get; init; } = [];
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
}
