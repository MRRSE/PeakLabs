using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class ArticleController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ArticleController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var model = new ArticleIndexViewModel
        {
            Articles = await _dbContext.Articles.AsNoTracking()
                .Where(article => article.Status == "published"
                    && (!categoryId.HasValue || article.CategoryId == categoryId.Value))
                .Include(article => article.Category)
                .Include(article => article.Author)
                .Include(article => article.Hashtags)
                .OrderByDescending(article => article.PublishedAt)
                .ToListAsync(),
            Categories = await _dbContext.ArticleCategories.AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync()
        };

        return View(model);
    }
    [HttpGet("Article/SingleArticle/{id:int}")]
    public async Task<IActionResult> SingleArticle(int id)
    {
        var article = await _dbContext.Articles.AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.Author)
            .Include(item => item.Sections.OrderBy(section => section.Order))
            .Include(item => item.Hashtags)
            .FirstOrDefaultAsync(item => item.Id == id && item.Status == "published");

        if (article is null) return NotFound();

        var relatedArticles = await _dbContext.Articles.AsNoTracking()
            .Where(item => item.Status == "published"
                && item.Id != id
                && item.CategoryId == article.CategoryId)
            .Include(item => item.Category)
            .Include(item => item.Author)
            .Include(item => item.Hashtags)
            .OrderByDescending(item => item.PublishedAt)
            .Take(3)
            .ToListAsync();

        return View(new ArticleDetailViewModel { Article = article, RelatedArticles = relatedArticles });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordView(int id)
    {
        var articleExists = await _dbContext.Articles.AsNoTracking()
            .Where(article => article.Id == id && article.Status == "published")
            .Select(article => new { article.Id, article.ViewCount })
            .SingleOrDefaultAsync();
        if (articleExists is null) return NotFound();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.ArticleViews.Add(new ArticleView { ArticleId = id, ViewedAt = DateTimeOffset.UtcNow });
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.Articles
            .Where(article => article.Id == id && article.Status == "published")
            .ExecuteUpdateAsync(setters => setters.SetProperty(article => article.ViewCount, article => article.ViewCount + 1));

        if (updated == 0)
        {
            await transaction.RollbackAsync();
            return NotFound();
        }

        await transaction.CommitAsync();
        var viewCount = await _dbContext.Articles.AsNoTracking()
            .Where(article => article.Id == id)
            .Select(article => article.ViewCount)
            .SingleAsync();

        return Ok(new { viewCount });
    }

}
