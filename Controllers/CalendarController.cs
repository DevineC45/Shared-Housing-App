using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Data;
using SharedHousingApp.Models;
using System.Linq;

namespace SharedHousingApp.Controllers
{
    public class CalendarController : Controller
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Calendar
        public IActionResult Index()
        {
            var events = _context.CalendarEvents
                .OrderBy(e => e.Date)
                .ToList();

            return View(events);
        }

        // GET: /Calendar/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            return View();
        }

        // POST: /Calendar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CalendarEvent calendarEvent)
        {
            Console.WriteLine("POST Create action hit");

            if (!ModelState.IsValid)
            {
                // 🔍 DEBUG: Show why validation failed
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Validation error in '{entry.Key}': {error.ErrorMessage}");
                    }
                }

                return View(calendarEvent);
            }

            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userName == null || userRole == null)
            {
                return RedirectToAction("Login", "Users");
            }

            calendarEvent.CreatedByName = userName;
            calendarEvent.EventType = userRole;

            _context.CalendarEvents.Add(calendarEvent);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpGet]
        public JsonResult GetEvents()
        {
            var events = _context.CalendarEvents
                .Select(e => new
                {
                    title = $"{(e.EventType == "Landlord" ? "🛠️" : "🎉")} {e.Title} - {e.CreatedByName}",
                    start = e.Date.ToString("yyyy-MM-dd"),
                }).ToList();

            return Json(events);
        }
    }
}