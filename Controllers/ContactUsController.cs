using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class ContactUsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ContactUsController(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public IActionResult Index() => View(new ContactMessage());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact-form")]
    public async Task<IActionResult> Index(ContactMessage input)
    {
        input.Name = input.Name?.Trim() ?? string.Empty;
        input.Email = input.Email?.Trim() ?? string.Empty;
        input.Subject = input.Subject?.Trim() ?? string.Empty;
        input.Message = input.Message?.Trim() ?? string.Empty;
        if (!ModelState.IsValid) return View(input);

        input.Id = 0;
        input.SentAt = DateTimeOffset.UtcNow;
        _dbContext.ContactMessages.Add(input);
        await _dbContext.SaveChangesAsync();
        TempData["ContactSuccess"] = "پیام شما ثبت شد. از اینکه با ما تماس گرفتید سپاسگزاریم.";
        return RedirectToAction(nameof(Index));
    }
}
