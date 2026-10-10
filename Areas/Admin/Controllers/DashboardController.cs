using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<PeakLabsUser> _userManager;

    public DashboardController(UserManager<PeakLabsUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewData["WelcomeName"] = string.IsNullOrWhiteSpace(user?.FirstName)
            ? user?.UserName ?? User.Identity?.Name ?? "کاربر"
            : user.FirstName;
        return View();
    }
}



