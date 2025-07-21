using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Models;
using SharedHousingApp.Data;

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
            // Only allow tenants
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant")
            {
                return RedirectToAction("Login", "Users");
            }

            var chores = _context.Chores.ToList();
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

            return View();
        }

        // POST: Chores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Chore chore)
        {
            if (ModelState.IsValid)
            {
                _context.Chores.Add(chore);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
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

            if (chore.RepeatWeekly)
            {
                chore.IsComplete = false;
            }
            else
            {
                chore.IsComplete = true;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}