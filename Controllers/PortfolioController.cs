using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class PortfolioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}