using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class PortfolioController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public PortfolioController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private const int PageSize = 9;

    public async Task<IActionResult> Index(int page = 1)
    {
        var totalCount = await _dbContext.Portfolio.AsNoTracking().CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await _dbContext.Portfolio.AsNoTracking()
            .OrderByDescending(item => item.AddedAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new PortfolioIndexViewModel
        {
            Items = items,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordView(int id)
    {
        var exists = await _dbContext.Portfolio.AsNoTracking().AnyAsync(item => item.Id == id);
        if (!exists) return NotFound();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.PortfolioViews.Add(new PortfolioView { PortfolioItemId = id, ViewedAt = DateTimeOffset.UtcNow });
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.Portfolio
            .Where(item => item.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.ViewCount, item => item.ViewCount + 1));

        if (updated == 0)
        {
            await transaction.RollbackAsync();
            return NotFound();
        }

        await transaction.CommitAsync();
        var viewCount = await _dbContext.Portfolio.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => item.ViewCount)
            .SingleAsync();

        return Ok(new { viewCount });
    }
}
