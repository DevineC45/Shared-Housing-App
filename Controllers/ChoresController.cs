using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Models;
using SharedHousingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace SharedHousingApp.Controllers
{
    public class ChoresController : Controller
    {
        private readonly AppDbContext _context;

        public ChoresController(AppDbContext context)
        {
            _context = context;
        }

        // Displays all chores (Tenant only)
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            var chores = _context.Chores
                .Include(c => c.AssignedToUser)
                .ToList();

            return View(chores);
        }

        // Displays chores assigned specifically to the logged-in tenant
        public IActionResult MyChores()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            int id = int.Parse(userId);

            var chores = _context.Chores
                .Include(c => c.AssignedToUser)
                .Where(c => c.AssignedToUserId == id)
                .ToList();

            return View(chores);
        }

        // Shows the Create Chore form (Tenant only)
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.Tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .ToList();

            return View();
        }

        // Handles form submission for creating a new chore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Chore chore)
        {
            if (ModelState.IsValid)
            {
                var firstTenant = _context.Users
                    .Where(u => u.Role == "Tenant")
                    .OrderBy(u => u.Id)
                    .FirstOrDefault();

                if (firstTenant == null)
                {
                    ModelState.AddModelError("", "No tenants found to assign the chore.");
                    return View(chore);
                }

                // Default values for a fresh chore
                chore.IsComplete = false; 
                if (chore.AssignedToUserId == 0)
                    chore.AssignedToUserId = firstTenant.Id;

                _context.Chores.Add(chore);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(chore);
        }

        // Shows the Edit Chore form for a specific chore (Tenant only)
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null) return NotFound();

            ViewBag.Tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .ToList();

            return View(chore);
        }

        // Handles form submission for editing a chore (safe field-level update)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Title,AssignedToUserId")] Chore input)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            if (id != input.Id) return BadRequest();

            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = _context.Users
                    .Where(u => u.Role == "Tenant")
                    .ToList();
                return View(chore);
            }

            // Update only allowed fields
            bool assigneeChanged = chore.AssignedToUserId != input.AssignedToUserId;

            chore.Title = input.Title;
            chore.AssignedToUserId = input.AssignedToUserId;

            // If reassigned, reset completion so the new assignee can complete it
            if (assigneeChanged)
            {
                chore.IsComplete = false;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Marks a chore as complete and rotates it to the next tenant
        [HttpPost]
        public IActionResult Complete(int id)
        {
            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null) return NotFound();

            var tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .OrderBy(u => u.Id)
                .ToList();

            if (tenants.Count == 0)
            {
                TempData["Message"] = "No tenants available to rotate the chore.";
                return RedirectToAction(nameof(Index));
            }

            // Find next assignee in the rotation
            var currentIndex = tenants.FindIndex(u => u.Id == chore.AssignedToUserId);
            if (currentIndex < 0) currentIndex = 0; // safety
            var nextIndex = (currentIndex + 1) % tenants.Count;
            var nextTenant = tenants[nextIndex];

            // Rotate
            chore.AssignedToUserId = nextTenant.Id;

            // After rotation, the chore should be pending for the new assignee
            chore.IsComplete = false;

            _context.SaveChanges();

            TempData["Message"] = $"Chore '{chore.Title}' is now assigned to {nextTenant.Name}!";
            return RedirectToAction(nameof(Index));
        }

        // Shows the Delete confirmation page for a chore
        public IActionResult Delete(int id)
        {
            var chore = _context.Chores
                .Include(c => c.AssignedToUser)
                .FirstOrDefault(c => c.Id == id);

            if (chore == null) return NotFound();

            return View(chore); // Razor confirmation page
        }

        // Handles deletion of a chore after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var chore = _context.Chores.Find(id);
            if (chore == null) return NotFound();

            _context.Chores.Remove(chore);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
