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

        // Displays all calendar events ordered by date
        public IActionResult Index()
        {
            var events = _context.CalendarEvents
                .OrderBy(e => e.Date)
                .ToList();

            return View(events);
        }

        // Shows the Create Event form (date can be pre-filled via query string)
        public IActionResult Create(DateTime? date)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Users");

            var model = new CalendarEvent
            {
                // if query has a date use it; otherwise today
                Date = (date?.Date) ?? DateTime.Today
            };

            return View(model);
        }

        // Handles form submission for creating a new calendar event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CalendarEvent calendarEvent)
        {
            Console.WriteLine("POST Create action hit");

            if (!ModelState.IsValid)
            {
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

        // Returns all events in JSON format (used by frontend calendar)
        [HttpGet]
        public JsonResult GetEvents()
        {
            var events = _context.CalendarEvents
                .Select(e => new
                {
                    id = e.Id,
                    // Only title + emoji, no "- Name"
                    title = e.Title,
                    start = e.Date.ToString("yyyy-MM-dd"),
                    description = e.Description,
                    createdBy = e.CreatedByName // sent for modal
                })
                .ToList();

            return Json(events);
        }

        // Shows the Edit Event form (only creator can edit)
        public IActionResult Edit(int id)
        {
            var calendarEvent = _context.CalendarEvents.Find(id);
            var userName = HttpContext.Session.GetString("UserName");

            if (calendarEvent == null || calendarEvent.CreatedByName != userName)
                return Unauthorized();

            return View(calendarEvent);
        }

        // Handles form submission for editing an event (only creator can edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CalendarEvent calendarEvent)
        {
            var userName = HttpContext.Session.GetString("UserName");

            if (calendarEvent.CreatedByName != userName)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Update(calendarEvent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(calendarEvent);
        }

        // Shows the Delete confirmation page for an event (only creator can delete)
        public IActionResult Delete(int id)
        {
            var calendarEvent = _context.CalendarEvents.Find(id);
            var userName = HttpContext.Session.GetString("UserName");

            if (calendarEvent == null || calendarEvent.CreatedByName != userName)
                return Unauthorized();

            return View(calendarEvent);
        }

        // Handles deletion of an event after confirmation (only creator can delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var calendarEvent = _context.CalendarEvents.Find(id);
            var userName = HttpContext.Session.GetString("UserName");

            if (calendarEvent == null || calendarEvent.CreatedByName != userName)
                return Unauthorized();

            _context.CalendarEvents.Remove(calendarEvent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
