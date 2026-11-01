using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models;
namespace PeakLabs.Infrastructure;
public sealed class DashboardActivityLogger
{
    private readonly ApplicationDbContext _db;
    public DashboardActivityLogger(ApplicationDbContext db) => _db = db;
    public async Task LogAsync(string message, string? userId = null)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-6);
        _db.DashboardActivities.Add(new DashboardActivity { Message = message, UserId = userId });
        await _db.DashboardActivities.Where(x => x.OccurredAt < cutoff).ExecuteDeleteAsync();
        await _db.SaveChangesAsync();
    }
}
