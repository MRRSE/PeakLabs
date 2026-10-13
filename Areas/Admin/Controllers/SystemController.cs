using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class SystemController : Controller
{

    public IActionResult Settings()
    {
        return View();
    }
}
