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
        var articles = await _dbContext.Articles.AsNoTracking()
            .Where(article => article.Status == "published")
            .Include(article => article.Category)
            .Include(article => article.Author)
            .OrderByDescending(article => article.PublishedAt)
            .Take(6)
            .ToListAsync();
        return View(articles);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
