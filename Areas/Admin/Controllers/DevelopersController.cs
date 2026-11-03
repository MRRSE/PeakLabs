using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
public class DevelopersController : Controller
{
    private const int MaxActiveDevelopers = 4;
    private const long MaxImageSize = 5 * 1024 * 1024;
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public DevelopersController(ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var developers = await _dbContext.Developers.AsNoTracking()
            .OrderByDescending(developer => developer.CreatedAt).ToListAsync();
        return View(developers);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxImageSize + 64 * 1024)]
    public async Task<IActionResult> Save(DeveloperInputModel input)
    {
        input.Name = input.Name?.Trim() ?? string.Empty;
        input.JobTitle = input.JobTitle?.Trim() ?? string.Empty;
        input.Email = input.Email?.Trim() ?? string.Empty;
        input.Bio = input.Bio?.Trim() ?? string.Empty;
        input.LinkedInUrl = NormalizeUrl(input.LinkedInUrl);
        input.GitHubUrl = NormalizeUrl(input.GitHubUrl);
        if (input.ProfileImage is { Length: > 0 } image)
        {
            if (image.Length > MaxImageSize)
                ModelState.AddModelError(nameof(input.ProfileImage), "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.");
            else if (!IsSupportedImage(image))
                ModelState.AddModelError(nameof(input.ProfileImage), "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }
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

        string? newImagePhysicalPath = null;
        var oldImagePath = developer.ProfileImagePath;
        try
        {
            if (input.ProfileImage is { Length: > 0 } profileImage)
            {
                var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "developers");
                Directory.CreateDirectory(uploadDirectory);
                newImagePhysicalPath = Path.Combine(uploadDirectory, fileName);
                await using var stream = System.IO.File.Create(newImagePhysicalPath);
                await profileImage.CopyToAsync(stream);
                developer.ProfileImagePath = $"/uploads/developers/{fileName}";
            }

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
            if (newImagePhysicalPath is not null) DeleteProfileImage(oldImagePath);
            return Ok(new { message = input.Id == 0 ? "توسعه‌دهنده اضافه شد." : "اطلاعات توسعه‌دهنده ذخیره شد." });
        }
        catch
        {
            if (newImagePhysicalPath is not null && System.IO.File.Exists(newImagePhysicalPath))
                System.IO.File.Delete(newImagePhysicalPath);
            throw;
        }
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
        DeleteProfileImage(developer.ProfileImagePath);
        return Ok(new { message = "توسعه‌دهنده حذف شد." });
    }

    private bool IsRelational() => _dbContext.Database.IsRelational();
    private string ValidationMessage() => string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
        .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)));
    private static string? NormalizeUrl(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool IsSafeSocialUrl(string? value) => value is null ||
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private void DeleteProfileImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        var path = Path.Combine(_environment.WebRootPath, "uploads", "developers", Path.GetFileName(relativePath));
        try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static bool IsSupportedImage(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var expectedType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };
        if (expectedType is null || !string.Equals(image.ContentType, expectedType, StringComparison.OrdinalIgnoreCase)) return false;
        Span<byte> header = stackalloc byte[12];
        using var stream = image.OpenReadStream();
        var bytesRead = stream.Read(header);
        return extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytesRead >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
