using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models;
using PeakLabs.Models.Identity;
using PeakLabs.Infrastructure;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager + "," + DashboardRoles.Editor)]
public class ArticlesController : Controller
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly DashboardActivityLogger _activityLogger;

    public ArticlesController(ApplicationDbContext dbContext, IWebHostEnvironment environment, UserManager<PeakLabsUser> userManager, DashboardActivityLogger activityLogger)
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
            dashboardUser.ArticlesLastViewedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(dashboardUser);
        }

        var isEditor = User.IsInRole(DashboardRoles.Editor);
        var currentUserId = _userManager.GetUserId(User);
        var articlesQuery = _dbContext.Articles.AsNoTracking()
                .Include(article => article.Category)
                .Include(article => article.Author)
                .Include(article => article.Sections.OrderBy(section => section.Order))
                .Include(article => article.Hashtags)
                .Where(article => !isEditor || article.AuthorId == currentUserId);

        var model = new ArticleAdminIndexViewModel
        {
            Articles = await articlesQuery
                .OrderByDescending(article => article.PublishedAt)
                .ToListAsync(),
            Categories = await _dbContext.ArticleCategories.AsNoTracking()
                .OrderBy(category => category.Name).ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager + "," + DashboardRoles.Editor)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageSize + 1024 * 1024)]
    public async Task<IActionResult> Create(CreateArticleInputModel input)
    {
        if (input.CoverImage is null || input.CoverImage.Length == 0)
        {
            ModelState.AddModelError(nameof(input.CoverImage), "انتخاب تصویر کاور الزامی است.");
        }
        else if (input.CoverImage.Length > MaxImageSize)
        {
            ModelState.AddModelError(nameof(input.CoverImage), "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.");
        }
        else if (!IsSupportedImage(input.CoverImage))
        {
            ModelState.AddModelError(nameof(input.CoverImage), "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }

        if (!await _dbContext.ArticleCategories.AnyAsync(category => category.Id == input.CategoryId))
        {
            ModelState.AddModelError(nameof(input.CategoryId), "دسته‌بندی انتخاب‌شده معتبر نیست.");
        }

        if (input.Sections.Count == 0)
        {
            ModelState.AddModelError(nameof(input.Sections), "حداقل یک بخش برای مقاله اضافه کنید.");
        }
        ValidateHashtags(input);

        for (var index = 0; index < input.Sections.Count; index++)
        {
            var section = input.Sections[index];
            if (!string.IsNullOrWhiteSpace(section.HeadingType) && section.HeadingType is not ("H2" or "H3" or "H4"))
            {
                ModelState.AddModelError($"Sections[{index}].HeadingType", "نوع عنوان بخش معتبر نیست.");
            }
            if (string.IsNullOrWhiteSpace(section.HeadingType) && !string.IsNullOrWhiteSpace(section.Title))
            {
                ModelState.AddModelError($"Sections[{index}].Title", "برای عنوان بخش، نوع عنوان را هم انتخاب کنید.");
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

        var extension = Path.GetExtension(input.CoverImage!.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "articles");
        Directory.CreateDirectory(uploadDirectory);
        var imagePath = Path.Combine(uploadDirectory, fileName);

        try
        {
            await using (var stream = System.IO.File.Create(imagePath))
            {
                await input.CoverImage.CopyToAsync(stream);
            }

            var article = new Article
            {
                Title = input.Title.Trim(),
                ShortDescription = input.ShortDescription,
                CoverImage = $"/uploads/articles/{fileName}",
                CategoryId = input.CategoryId,
                AuthorId = _userManager.GetUserId(User),
                Status = input.Status,
                PublishedAt = DateTimeOffset.UtcNow,
                Hashtags = MapHashtags(input),
                Sections = input.Sections.Select((section, index) => new ArticleSection
                {
                    HeadingType = string.IsNullOrWhiteSpace(section.HeadingType) ? null : section.HeadingType,
                    Title = string.IsNullOrWhiteSpace(section.Title) ? null : section.Title.Trim(),
                    Content = section.Content,
                    Order = index + 1
                }).ToList()
            };

            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();
            await _activityLogger.LogAsync($"مقاله «{article.Title}» ثبت شده", _userManager.GetUserId(User));
            return Ok(new { message = "مقاله با موفقیت ذخیره شد." });
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
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager + "," + DashboardRoles.Editor)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageSize + 1024 * 1024)]
    public async Task<IActionResult> Update(int id, CreateArticleInputModel input)
    {
        var article = await _dbContext.Articles
            .Include(item => item.Sections)
            .Include(item => item.Hashtags)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (article is null)
        {
            return NotFound(new { message = "مقاله موردنظر پیدا نشد." });
        }
        if (User.IsInRole(DashboardRoles.Editor) && article.AuthorId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        if (input.CoverImage is { Length: > 0 } image)
        {
            if (image.Length > MaxImageSize)
                ModelState.AddModelError(nameof(input.CoverImage), "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.");
            else if (!IsSupportedImage(image))
                ModelState.AddModelError(nameof(input.CoverImage), "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }

        if (!await _dbContext.ArticleCategories.AnyAsync(category => category.Id == input.CategoryId))
            ModelState.AddModelError(nameof(input.CategoryId), "دسته‌بندی انتخاب‌شده معتبر نیست.");
        ValidateSections(input);
        ValidateHashtags(input);
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ValidationMessage() });
        }

        var oldCover = article.CoverImage;
        string? newImagePath = null;
        try
        {
            if (input.CoverImage is { Length: > 0 } newImage)
            {
                var extension = Path.GetExtension(newImage.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "articles");
                Directory.CreateDirectory(uploadDirectory);
                newImagePath = Path.Combine(uploadDirectory, fileName);
                await using var stream = System.IO.File.Create(newImagePath);
                await newImage.CopyToAsync(stream);
                article.CoverImage = $"/uploads/articles/{fileName}";
            }

            article.Title = input.Title.Trim();
            article.ShortDescription = input.ShortDescription;
            article.CategoryId = input.CategoryId;
            article.Status = input.Status;
            _dbContext.ArticleSections.RemoveRange(article.Sections);
            article.Sections = MapSections(input);
            SyncHashtags(article, input);
            await _dbContext.SaveChangesAsync();
            await _activityLogger.LogAsync($"مقاله «{article.Title}» ویرایش شده", _userManager.GetUserId(User));
            if (newImagePath is not null) DeleteArticleImage(oldCover);
            return Ok(new { message = "تغییرات مقاله ذخیره شد." });
        }
        catch
        {
            if (newImagePath is not null && System.IO.File.Exists(newImagePath)) System.IO.File.Delete(newImagePath);
            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _dbContext.Articles.FindAsync(id);
        if (article is null) return NotFound(new { message = "مقاله موردنظر پیدا نشد." });
        if (User.IsInRole(DashboardRoles.Editor) && article.AuthorId != _userManager.GetUserId(User)) return Forbid();

        _dbContext.Articles.Remove(article);
        await _dbContext.SaveChangesAsync();
        await _activityLogger.LogAsync($"مقاله «{article.Title}» حذف شده", _userManager.GetUserId(User));
        DeleteArticleImage(article.CoverImage);
        return Ok(new { message = "مقاله حذف شد." });
    }

    private void ValidateSections(CreateArticleInputModel input)
    {
        if (input.Sections.Count == 0) return;
        for (var index = 0; index < input.Sections.Count; index++)
        {
            var section = input.Sections[index];
            if (!string.IsNullOrWhiteSpace(section.HeadingType) && section.HeadingType is not ("H2" or "H3" or "H4"))
                ModelState.AddModelError($"Sections[{index}].HeadingType", "نوع عنوان بخش معتبر نیست.");
            if (string.IsNullOrWhiteSpace(section.HeadingType) && !string.IsNullOrWhiteSpace(section.Title))
                ModelState.AddModelError($"Sections[{index}].Title", "برای عنوان بخش، نوع عنوان را هم انتخاب کنید.");
        }
    }

    private List<ArticleSection> MapSections(CreateArticleInputModel input) => input.Sections
        .Select((section, index) => new ArticleSection
        {
            HeadingType = string.IsNullOrWhiteSpace(section.HeadingType) ? null : section.HeadingType,
            Title = string.IsNullOrWhiteSpace(section.Title) ? null : section.Title.Trim(),
            Content = section.Content,
            Order = index + 1
        }).ToList();

    private void ValidateHashtags(CreateArticleInputModel input)
    {
        var rawTags = SplitHashtags(input.Hashtags);
        var tags = ParseHashtags(input.Hashtags);
        if (tags.Count > 8)
            ModelState.AddModelError(nameof(input.Hashtags), "حداکثر ۸ هشتگ برای هر مقاله مجاز است.");

        foreach (var rawTag in rawTags)
        {
            var tag = rawTag.TrimStart('#');
            if (tag.Length is 0 or > 40 || tag.Any(character =>
                    !char.IsLetterOrDigit(character) && character is not ('_' or '-')))
            {
                ModelState.AddModelError(nameof(input.Hashtags), "هشتگ‌ها فقط می‌توانند شامل حروف، عدد، خط تیره و زیرخط باشند و حداکثر ۴۰ نویسه داشته باشند.");
                break;
            }
        }
    }

    private static List<string> SplitHashtags(string? raw) => (raw ?? string.Empty)
        .Split([',', ';', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    private static List<string> ParseHashtags(string? raw) => SplitHashtags(raw)
        .Select(tag => tag.TrimStart('#').Trim())
        .Where(tag => tag.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static List<ArticleHashtag> MapHashtags(CreateArticleInputModel input) => ParseHashtags(input.Hashtags)
        .Select(tag => new ArticleHashtag { Tag = tag })
        .ToList();

    private void SyncHashtags(Article article, CreateArticleInputModel input)
    {
        var desiredTags = ParseHashtags(input.Hashtags).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingTags = article.Hashtags.ToDictionary(hashtag => hashtag.Tag, StringComparer.OrdinalIgnoreCase);

        _dbContext.ArticleHashtags.RemoveRange(article.Hashtags
            .Where(hashtag => !desiredTags.Contains(hashtag.Tag)));

        foreach (var tag in desiredTags.Where(tag => !existingTags.ContainsKey(tag)))
            article.Hashtags.Add(new ArticleHashtag { ArticleId = article.Id, Tag = tag });
    }

    private string ValidationMessage() => string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
        .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)));

    private void DeleteArticleImage(string relativePath)
    {
        var path = Path.Combine(_environment.WebRootPath, "uploads", "articles", Path.GetFileName(relativePath));
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
