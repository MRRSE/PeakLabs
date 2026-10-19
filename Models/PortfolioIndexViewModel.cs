namespace PeakLabs.Models;

public class PortfolioIndexViewModel
{
    public IReadOnlyList<PortfolioItem> Items { get; init; } = Array.Empty<PortfolioItem>();
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public int TotalItems { get; init; }
}
