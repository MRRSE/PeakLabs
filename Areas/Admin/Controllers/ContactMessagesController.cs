using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
public sealed class ContactMessagesController : Controller
{
    private const int PageSize = 25;
    private readonly ApplicationDbContext _dbContext;

    public ContactMessagesController(ApplicationDbContext dbContext) => _dbContext = dbContext;

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

        message.IsAnswered = answered;
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = answered ? "پیام به پاسخ‌داده‌شده‌ها منتقل شد." : "پیام به پیام‌های در انتظار منتقل شد." });
    }
}
