using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedHousingApp.Data;
using SharedHousingApp.Models;

namespace SharedHousingApp.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Tenant") return RedirectToAction("Login", "Users");

            var expenses = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .Where(e => !e.IsSettled)
                .ToList();

            return View(expenses);
        }

        // GET: Expenses/MyExpenses
        public IActionResult MyExpenses()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            int id = int.Parse(userId);

            var myExpenses = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.PaidByUserId == id && !e.IsSettled)
                .ToList();

            return View(myExpenses);
        }

        // GET: Expenses/SettledExpenses
        public IActionResult SettledExpenses()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            int id = int.Parse(userId);

            var settled = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.PaidByUserId == id && e.IsSettled)
                .ToList();

            return View(settled);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Tenant" || string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            int id = int.Parse(userId);

            var tenants = _context.Users
                .Where(u => u.Role == "Tenant" && u.Id != id)
                .OrderBy(u => u.Name)
                .ToList();

            ViewBag.Tenants = tenants;

            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense expense, List<int> SelectedUserIds)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                expense.PaidByUserId = int.Parse(userId);
                expense.SharedWithUsers = _context.Users
                    .Where(u => SelectedUserIds.Contains(u.Id))
                    .ToList();

                _context.Expenses.Add(expense);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tenants = _context.Users
                .Where(u => u.Role == "Tenant")
                .OrderBy(u => u.Name)
                .ToList();

            return View(expense);
        }

        // GET: Expenses/Settle/5
        public IActionResult Settle(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null) return NotFound();

            var userId = HttpContext.Session.GetString("UserId");
            if (expense.PaidByUserId != int.Parse(userId!)) return Unauthorized();

            return View(expense);
        }

        // POST: Expenses/Settle/5
        [HttpPost, ActionName("Settle")]
        [ValidateAntiForgeryToken]
        public IActionResult SettleConfirmed(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();

            expense.IsSettled = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(MyExpenses));
        }

        // GET: Expenses/Delete/5
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null) return NotFound();

            return View(expense); // Confirmation view
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}