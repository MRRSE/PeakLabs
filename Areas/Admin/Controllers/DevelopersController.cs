using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DevelopersController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
