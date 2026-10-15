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

    public IActionResult Index()
    {
        return View();
    }
    [HttpGet("Article/SingleArticle/{id:int}")]
    public async Task<IActionResult> SingleArticle(int id)
    {
        var article = await _dbContext.Articles.AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.Author)
            .Include(item => item.Sections.OrderBy(section => section.Order))
            .FirstOrDefaultAsync(item => item.Id == id && item.Status == "published");

        return article is null ? NotFound() : View(article);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordView(int id)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        if (!await _dbContext.Articles.AnyAsync(article => article.Id == id && article.Status == "published"))
            return NotFound();

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
        return NoContent();
    }

}
