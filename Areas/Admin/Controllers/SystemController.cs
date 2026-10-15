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
        return View();
    }

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
