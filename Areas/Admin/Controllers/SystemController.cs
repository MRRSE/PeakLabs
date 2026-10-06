using Microsoft.AspNetCore.Mvc;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
public class SystemController : Controller
{

    public IActionResult Settings()
    {
        return View();
    }
}