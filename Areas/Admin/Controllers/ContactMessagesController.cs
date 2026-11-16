using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Infrastructure;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager + "," + DashboardRoles.CustomerSupport)]
public sealed class ContactMessagesController : Controller
{
    private const int PageSize = 25;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly DashboardActivityLogger _activityLogger;

    public ContactMessagesController(
        ApplicationDbContext dbContext,
        UserManager<PeakLabsUser> userManager,
        DashboardActivityLogger activityLogger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _activityLogger = activityLogger;
    }

    public async Task<IActionResult> Index(bool answered = false, int page = 1)
    {
        page = Math.Max(page, 1);
        var allMessages = _dbContext.ContactMessages.AsNoTracking();
        var pendingCount = await allMessages.CountAsync(message => !message.IsAnswered);
        var answeredCount = await allMessages.CountAsync(message => message.IsAnswered);
        var query = allMessages.Where(message => message.IsAnswered == answered)
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id);
        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        page = Math.Min(page, totalPages);
        var messages = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
        return View(new ContactMessagesIndexViewModel(messages, answered, pendingCount, answeredCount, page, totalPages));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAnswered(int id, bool answered)
    {
        var message = await _dbContext.ContactMessages.FindAsync(id);
        if (message is null) return NotFound(new { message = "پیام موردنظر پیدا نشد." });

        if (message.IsAnswered != answered)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            message.IsAnswered = answered;
            await _dbContext.SaveChangesAsync();
            var action = answered ? "به پاسخ‌داده‌شده‌ها منتقل شد" : "به پیام‌های در انتظار بازگردانده شد";
            await _activityLogger.LogAsync(
                $"پیام «{message.Subject}» از {message.Name} {action}.",
                _userManager.GetUserId(User));
            await transaction.CommitAsync();
        }
        return Ok(new { message = answered ? "پیام به پاسخ‌داده‌شده‌ها منتقل شد." : "پیام به پیام‌های در انتظار منتقل شد." });
    }
}
