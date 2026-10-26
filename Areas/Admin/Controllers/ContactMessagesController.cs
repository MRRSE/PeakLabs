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

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _dbContext.ContactMessages.AsNoTracking()
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id);
        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        page = Math.Min(page, totalPages);
        var messages = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
        return View(new ContactMessagesIndexViewModel(messages, page, totalPages));
    }
}
