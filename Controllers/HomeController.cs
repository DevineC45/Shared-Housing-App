using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Models;

namespace SharedHousingApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId != null)
        {
            return RedirectToAction("Dashboard");
        }

        return View(); // Public homepage
    }

    public IActionResult Dashboard()
    {
        var role = HttpContext.Session.GetString("UserRole");
        ViewData["Role"] = role;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
