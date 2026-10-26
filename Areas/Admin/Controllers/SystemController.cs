using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class SystemController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public SystemController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ArticleCategories"] = await _dbContext.ArticleCategories.AsNoTracking()
            .OrderBy(category => category.Name).ToListAsync();
        ViewData["SiteSettings"] = await _dbContext.SiteSettings.AsNoTracking().FirstOrDefaultAsync(settings => settings.Id == 1)
            ?? new SiteSettings();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings([FromBody] SiteSettings input)
    {
        if (string.IsNullOrWhiteSpace(input.SiteTitle) || input.SiteTitle.Length > 70
            || string.IsNullOrWhiteSpace(input.SiteDescription) || input.SiteDescription.Length > 300
            || (input.SeoTitle?.Length ?? 0) > 60 || (input.SeoDescription?.Length ?? 0) > 160
            || (input.Address?.Length ?? 0) > 300)
            return BadRequest(new { message = "مقادیر واردشده معتبر نیستند." });

        var settings = await _dbContext.SiteSettings.FindAsync(1);
        if (settings is null)
        {
            settings = new SiteSettings { Id = 1 };
            _dbContext.SiteSettings.Add(settings);
        }

        settings.SiteTitle = input.SiteTitle.Trim();
        settings.SiteDescription = input.SiteDescription.Trim();
        settings.SiteUrl = Clean(input.SiteUrl);
        settings.SeoTitle = Clean(input.SeoTitle);
        settings.SeoDescription = Clean(input.SeoDescription);
        settings.OgImage = Clean(input.OgImage);
        settings.ContactEmail = Clean(input.ContactEmail);
        settings.ContactPhone = Clean(input.ContactPhone);
        settings.Address = Clean(input.Address);
        settings.Linkedin = Clean(input.Linkedin);
        settings.Github = Clean(input.Github);
        settings.Instagram = Clean(input.Instagram);
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "تغییرات ذخیره شد." });
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name)
    {
        name = name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 80)
        {
            TempData["CategoryError"] = "نام دسته‌بندی باید بین ۱ تا ۸۰ نویسه باشد.";
            return RedirectToAction(nameof(Index));
        }

        if (await _dbContext.ArticleCategories.AnyAsync(category => category.Name == name))
        {
            TempData["CategoryError"] = "این دسته‌بندی از قبل وجود دارد.";
            return RedirectToAction(nameof(Index));
        }

        _dbContext.ArticleCategories.Add(new ArticleCategory { Name = name });
        await _dbContext.SaveChangesAsync();
        TempData["CategorySuccess"] = "دسته‌بندی اضافه شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _dbContext.ArticleCategories.FindAsync(id);
        if (category is null) return NotFound();

        if (await _dbContext.Articles.AnyAsync(article => article.CategoryId == id))
        {
            TempData["CategoryError"] = "این دسته‌بندی به مقاله متصل است و حذف نمی‌شود.";
            return RedirectToAction(nameof(Index));
        }

        _dbContext.ArticleCategories.Remove(category);
        await _dbContext.SaveChangesAsync();
        TempData["CategorySuccess"] = "دسته‌بندی حذف شد.";
        return RedirectToAction(nameof(Index));
    }
}
