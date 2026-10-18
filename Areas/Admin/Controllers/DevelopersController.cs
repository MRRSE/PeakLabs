using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DevelopersController : Controller
{
    private const int MaxActiveDevelopers = 4;
    private readonly ApplicationDbContext _dbContext;

    public DevelopersController(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Index()
    {
        var developers = await _dbContext.Developers.AsNoTracking()
            .OrderByDescending(developer => developer.CreatedAt).ToListAsync();
        return View(developers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(DeveloperInputModel input)
    {
        input.Name = input.Name?.Trim() ?? string.Empty;
        input.JobTitle = input.JobTitle?.Trim() ?? string.Empty;
        input.Email = input.Email?.Trim() ?? string.Empty;
        input.Bio = input.Bio?.Trim() ?? string.Empty;
        input.LinkedInUrl = NormalizeUrl(input.LinkedInUrl);
        input.GitHubUrl = NormalizeUrl(input.GitHubUrl);
        if (!IsSafeSocialUrl(input.LinkedInUrl) || !IsSafeSocialUrl(input.GitHubUrl))
            ModelState.AddModelError("SocialUrl", "پیوند شبکه‌های اجتماعی باید با http یا https شروع شود.");

        if (!ModelState.IsValid)
            return BadRequest(new { message = ValidationMessage() });

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsRelational()
            ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
        var developer = input.Id == 0 ? new Developer() : await _dbContext.Developers.FindAsync(input.Id);
        if (developer is null)
            return NotFound(new { message = "توسعه‌دهندهٔ موردنظر پیدا نشد." });

        if (input.IsActive && !developer.IsActive &&
            await _dbContext.Developers.CountAsync(item => item.IsActive) >= MaxActiveDevelopers)
            return Conflict(new { message = "حداکثر چهار توسعه‌دهنده می‌توانند در سایت فعال باشند." });

        developer.Name = input.Name;
        developer.JobTitle = input.JobTitle;
        developer.Email = input.Email;
        developer.Bio = input.Bio;
        developer.LinkedInUrl = input.LinkedInUrl;
        developer.GitHubUrl = input.GitHubUrl;
        developer.IsActive = input.IsActive;
        if (input.Id == 0)
        {
            developer.CreatedAt = DateTimeOffset.UtcNow;
            _dbContext.Developers.Add(developer);
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { message = input.Id == 0 ? "توسعه‌دهنده اضافه شد." : "اطلاعات توسعه‌دهنده ذخیره شد." });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsRelational()
            ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
        var developer = await _dbContext.Developers.FindAsync(id);
        if (developer is null) return NotFound(new { message = "توسعه‌دهندهٔ موردنظر پیدا نشد." });

        if (!developer.IsActive && await _dbContext.Developers.CountAsync(item => item.IsActive) >= MaxActiveDevelopers)
            return Conflict(new { message = "حداکثر چهار توسعه‌دهنده می‌توانند در سایت فعال باشند." });

        developer.IsActive = !developer.IsActive;
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { message = developer.IsActive ? "توسعه‌دهنده در سایت نمایش داده می‌شود." : "نمایش توسعه‌دهنده غیرفعال شد." });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var developer = await _dbContext.Developers.FindAsync(id);
        if (developer is null) return NotFound(new { message = "توسعه‌دهندهٔ موردنظر پیدا نشد." });
        _dbContext.Developers.Remove(developer);
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "توسعه‌دهنده حذف شد." });
    }

    private bool IsRelational() => _dbContext.Database.IsRelational();
    private string ValidationMessage() => string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
        .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)));
    private static string? NormalizeUrl(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool IsSafeSocialUrl(string? value) => value is null ||
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
