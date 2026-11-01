using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var publishedArticles = _dbContext.Articles.AsNoTracking()
            .Where(article => article.Status == "published");
        ViewBag.PublishedArticleCount = await _dbContext.Articles.AsNoTracking()
            .CountAsync(article => article.Status == "published");
        ViewBag.PortfolioItemCount = await _dbContext.Portfolio.AsNoTracking().CountAsync();
        ViewBag.PortfolioItems = await _dbContext.Portfolio.AsNoTracking()
            .OrderByDescending(item => item.AddedAt)
            .ThenByDescending(item => item.Id)
            .Take(6)
            .ToListAsync();

        var model = new HomeIndexViewModel
        {
            Articles = await publishedArticles
                .Include(article => article.Category)
                .Include(article => article.Author)
                .Include(article => article.Hashtags)
                .OrderByDescending(article => article.PublishedAt)
                .Take(6)
                .ToListAsync(),
            Categories = await _dbContext.ArticleCategories.AsNoTracking()
                .OrderBy(category => category.Id)
                .Select(category => new ArticleCategorySummary
                {
                    Id = category.Id,
                    Name = category.Name,
                    ArticleCount = category.Articles.Count(article => article.Status == "published")
                })
                .ToListAsync()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
