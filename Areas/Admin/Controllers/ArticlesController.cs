using Microsoft.AspNetCore.Mvc;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
public class ArticlesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}