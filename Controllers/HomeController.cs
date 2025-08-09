using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Models;

namespace SharedHousingApp.Controllers
{
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

            // Mark this as the landing page so _Layout.cshtml shows public nav links
            ViewData["BodyClass"] = "landing-page";
            return View(); 
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

        public IActionResult About()
        {
            ViewData["BodyClass"] = "landing-page";
            return View();
        }

        public IActionResult Features()
        {
            ViewData["BodyClass"] = "landing-page";
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["BodyClass"] = "landing-page";
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string Name, string Email, string Message)
        {
            // Fake success for demonstration purposes
            TempData["ContactOk"] = true;
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
