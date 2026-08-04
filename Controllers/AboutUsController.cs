using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class AboutUsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}