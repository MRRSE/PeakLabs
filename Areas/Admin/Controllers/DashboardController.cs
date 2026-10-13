using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public DashboardController(UserManager<PeakLabsUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewData["WelcomeName"] = string.IsNullOrWhiteSpace(user?.FirstName)
            ? user?.UserName ?? User.Identity?.Name ?? "کاربر"
            : user.FirstName;
        ViewData["ArticleCategories"] = await _dbContext.ArticleCategories.AsNoTracking()
            .OrderBy(category => category.Name).ToListAsync();
        return View();
    }
}



