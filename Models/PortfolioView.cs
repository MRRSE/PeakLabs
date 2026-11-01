namespace PeakLabs.Models;

public class PortfolioView
{
    public int Id { get; set; }
    public int PortfolioItemId { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
    public PortfolioItem PortfolioItem { get; set; } = null!;
}
