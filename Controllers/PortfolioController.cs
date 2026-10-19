using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class PortfolioController : Controller
{
    private const int PageSize = 9;
    private readonly ApplicationDbContext _dbContext;

    public PortfolioController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var totalItems = await _dbContext.Portfolio.AsNoTracking().CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        var currentPage = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);
        var items = await _dbContext.Portfolio.AsNoTracking()
            .OrderByDescending(item => item.AddedAt)
            .ThenByDescending(item => item.Id)
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new PortfolioIndexViewModel
        {
            Items = items,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalItems = totalItems
        });
    }
}
