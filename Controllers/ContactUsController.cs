using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Models;

namespace PeakLabs.Controllers;

public class ContactUsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}