using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DashboardRoles.SuperAdmin + "," + DashboardRoles.ContentManager)]
public class DevelopersController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
