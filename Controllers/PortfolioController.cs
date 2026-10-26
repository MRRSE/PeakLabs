using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;

namespace PeakLabs.Controllers;

public class PortfolioController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public PortfolioController(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Index()
    {
        var items = await _dbContext.Portfolio.AsNoTracking()
            .OrderByDescending(item => item.AddedAt).ToListAsync();
        return View(items);
    }
}
