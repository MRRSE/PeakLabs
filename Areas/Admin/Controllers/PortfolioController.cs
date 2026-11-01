using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models;
using PeakLabs.Models.Identity;
using PeakLabs.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
public class PortfolioController : Controller
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly DashboardActivityLogger _activityLogger;

    public PortfolioController(ApplicationDbContext dbContext, IWebHostEnvironment environment, UserManager<PeakLabsUser> userManager, DashboardActivityLogger activityLogger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _userManager = userManager;
        _activityLogger = activityLogger;
    }

    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager + "," + DashboardRoles.Editor)]
    public async Task<IActionResult> Index()
    {
        var dashboardUser = await _userManager.GetUserAsync(User);
        if (dashboardUser is not null)
        {
            dashboardUser.PortfolioLastViewedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(dashboardUser);
        }

        var items = await _dbContext.Portfolio
            .AsNoTracking()
            .OrderByDescending(item => item.AddedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageSize + 64 * 1024)]
    public async Task<IActionResult> Create(CreatePortfolioItemInputModel input)
    {
        if (input.ProjectImage is null || input.ProjectImage.Length == 0)
        {
            ModelState.AddModelError(nameof(input.ProjectImage), "انتخاب تصویر پروژه الزامی است.");
        }
        else if (input.ProjectImage.Length > MaxImageSize)
        {
            ModelState.AddModelError(nameof(input.ProjectImage), "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.");
        }
        else if (!IsSupportedImage(input.ProjectImage))
        {
            ModelState.AddModelError(nameof(input.ProjectImage), "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)))
            });
        }

        var extension = Path.GetExtension(input.ProjectImage!.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "portfolio");
        Directory.CreateDirectory(uploadDirectory);
        var imagePath = Path.Combine(uploadDirectory, fileName);
        var relativeImagePath = $"/uploads/portfolio/{fileName}";

        try
        {
            await using (var stream = System.IO.File.Create(imagePath))
            {
                await input.ProjectImage.CopyToAsync(stream);
            }

            var item = new PortfolioItem
            {
                ProjectImagePath = relativeImagePath,
                ProjectName = input.ProjectName.Trim(),
                ProjectDescription = input.ProjectDescription.Trim(),
                ProjectLink = input.ProjectLink.Trim(),
                AddedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Portfolio.Add(item);
            await _dbContext.SaveChangesAsync();
            await _activityLogger.LogAsync($"نمونه‌کار «{item.ProjectName}» اضافه شده", _userManager.GetUserId(User));
            return Ok(new { message = "نمونه‌کار با موفقیت ذخیره شد." });
        }
        catch
        {
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageSize + 64 * 1024)]
    public async Task<IActionResult> Update(UpdatePortfolioItemInputModel input)
    {
        var item = await _dbContext.Portfolio.FindAsync(input.Id);
        if (item is null)
        {
            return NotFound(new { message = "نمونه‌کار موردنظر پیدا نشد." });
        }

        if (input.ProjectImage is { Length: > 0 } image)
        {
            if (image.Length > MaxImageSize)
            {
                ModelState.AddModelError(nameof(input.ProjectImage), "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.");
            }
            else if (!IsSupportedImage(image))
            {
                ModelState.AddModelError(nameof(input.ProjectImage), "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)))
            });
        }

        var oldImagePath = item.ProjectImagePath;
        string? newImagePhysicalPath = null;
        try
        {
            if (input.ProjectImage is { Length: > 0 } newImage)
            {
                var extension = Path.GetExtension(newImage.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "portfolio");
                Directory.CreateDirectory(uploadDirectory);
                newImagePhysicalPath = Path.Combine(uploadDirectory, fileName);
                await using var stream = System.IO.File.Create(newImagePhysicalPath);
                await newImage.CopyToAsync(stream);
                item.ProjectImagePath = $"/uploads/portfolio/{fileName}";
            }

            item.ProjectName = input.ProjectName.Trim();
            item.ProjectDescription = input.ProjectDescription.Trim();
            item.ProjectLink = input.ProjectLink.Trim();
            await _dbContext.SaveChangesAsync();
            await _activityLogger.LogAsync($"نمونه‌کار «{item.ProjectName}» ویرایش شده", _userManager.GetUserId(User));

            if (newImagePhysicalPath is not null)
            {
                DeletePortfolioImage(oldImagePath);
            }

            return Ok(new { message = "تغییرات نمونه‌کار ذخیره شد." });
        }
        catch
        {
            if (newImagePhysicalPath is not null && System.IO.File.Exists(newImagePhysicalPath))
            {
                System.IO.File.Delete(newImagePhysicalPath);
            }

            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _dbContext.Portfolio.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { message = "نمونه‌کار موردنظر پیدا نشد." });
        }

        _dbContext.Portfolio.Remove(item);
        await _dbContext.SaveChangesAsync();
        await _activityLogger.LogAsync($"نمونه‌کار «{item.ProjectName}» حذف شده", _userManager.GetUserId(User));
        DeletePortfolioImage(item.ProjectImagePath);

        return Ok(new { message = "نمونه‌کار حذف شد." });
    }

    private void DeletePortfolioImage(string relativePath)
    {
        var imagePath = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "portfolio",
            Path.GetFileName(relativePath));
        try
        {
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
        catch (IOException)
        {
            // A locked file can be cleaned up later without undoing the database change.
        }
        catch (UnauthorizedAccessException)
        {
            // A file permission issue must not undo the database change.
        }
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
        if (expectedType is null || !string.Equals(image.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

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
