using PeakLabs.Models.Identity;
namespace PeakLabs.Models;
public class DashboardActivity
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public PeakLabsUser? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
