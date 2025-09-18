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

        // Displays all announcements, ordered by most recent first
        public IActionResult Index()
        {
            var announcements = _context.Announcements
                .OrderByDescending(a => a.PostedAt)
                .ToList();

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View(announcements);
        }

        // Shows the Create Announcement form (Landlord only)
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            return View();
        }

        // Handles form submission for creating a new announcement (Landlord only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Announcement announcement)
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            if (ModelState.IsValid)
            {
                announcement.PostedAt = DateTime.Now;
                _context.Announcements.Add(announcement);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(announcement);
        }

        // Shows the Edit Announcement form for a specific announcement (Landlord only)
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // Handles form submission for editing an existing announcement (Landlord only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Announcement announcement)
        {
            if (id != announcement.Id) return BadRequest();
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            if (ModelState.IsValid)
            {
                _context.Update(announcement);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(announcement);
        }

        // Shows the Delete confirmation page for a specific announcement (Landlord only)
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // Handles the deletion of an announcement after confirmation (Landlord only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Landlord")
                return RedirectToAction("Dashboard", "Home");

            var announcement = _context.Announcements.Find(id);
            if (announcement == null) return NotFound();

            _context.Announcements.Remove(announcement);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
