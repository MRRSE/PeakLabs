using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models;
using PeakLabs.Data;
using Microsoft.EntityFrameworkCore;

namespace PeakLabs.Controllers;

public class AboutUsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AboutUsController(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IActionResult> Index()
    {
        var developers = await _dbContext.Developers.AsNoTracking()
            .Where(developer => developer.IsActive)
            .OrderBy(developer => developer.Name)
            .ToListAsync();
        return View(developers);
    }
}
