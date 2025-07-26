using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Data;
using SharedHousingApp.Models;
using System.Linq;

namespace SharedHousingApp.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;

        public AnnouncementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Announcements
        public IActionResult Index()
        {
            // Announcements ordered by newest first
            var announcements = _context.Announcements
                .OrderByDescending(a => a.PostedAt)
                .ToList();

            return View(announcements);
        }

        // GET: /Announcements/Create
        public IActionResult Create()
        {
            // Only allow access if user is logged in as landlord
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }

        // POST: /Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Announcement announcement)
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            if (ModelState.IsValid)
            {
                announcement.PostedAt = DateTime.Now;
                _context.Announcements.Add(announcement);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(announcement);
        }
    }
}