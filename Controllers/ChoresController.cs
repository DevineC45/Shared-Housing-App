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

        // GET: Chores
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

        // GET: Chores/Schedule
        public IActionResult Schedule()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Users");
            }

            var chores = _context.Chores
                .Include(c => c.AssignedToUser)
                .ToList();

            return View(chores);
        }

        // GET: Chores/Create
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

        // POST: Chores/Create
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

                chore.AssignedToUserId = firstTenant.Id;

                _context.Chores.Add(chore);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(chore);
        }

        // GET: Chores/Edit/5
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

        // POST: Chores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Chore chore)
        {
            if (id != chore.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(chore);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .ToList();

            return View(chore);
        }

        // POST: Chores/Complete/5
        [HttpPost]
        public IActionResult Complete(int id)
        {
            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null) return NotFound();

            // Find all tenants
            var tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .OrderBy(u => u.Id)
                .ToList();

            var currentIndex = tenants.FindIndex(u => u.Id == chore.AssignedToUserId);
            var nextIndex = (currentIndex + 1) % tenants.Count;
            var nextTenant = tenants[nextIndex];

            chore.AssignedToUserId = nextTenant.Id;
            chore.IsComplete = !chore.RepeatWeekly;

            _context.SaveChanges();

            // ✅ Success message for user feedback
            TempData["Message"] = $"Chore '{chore.Title}' is now assigned to {nextTenant.Name}!";

            return RedirectToAction(nameof(Index));
        }
    }
}
