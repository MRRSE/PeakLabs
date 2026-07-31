using Microsoft.AspNetCore.Mvc;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
public class AdministratorsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}