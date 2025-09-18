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

        // --- helpers ---

        // Gets the current logged-in user's ID from session
        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out var id)) return id;
            return null;
        }

        // Populates ViewBag.Housemates with tenants except the current user
        private void PopulateHousematesExceptCurrent()
        {
            var me = GetCurrentUserId() ?? -1;

            ViewBag.Housemates = _context.Users
                .Where(u => u.Role == "Tenant" && u.Id != me)
                .OrderBy(u => u.Name)
                .ToList();
        }

        // Displays all unsettled expenses
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

        // Displays unsettled expenses created by the logged-in user
        public IActionResult MyExpenses()
        {
            var id = GetCurrentUserId();
            if (id is null) return RedirectToAction("Login", "Users");

            var myExpenses = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.PaidByUserId == id && !e.IsSettled)
                .ToList();

            return View(myExpenses);
        }

        // Displays settled expenses created by the logged-in user
        public IActionResult SettledExpenses()
        {
            var id = GetCurrentUserId();
            if (id is null) return RedirectToAction("Login", "Users");

            var settled = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.PaidByUserId == id && e.IsSettled)
                .ToList();

            return View(settled);
        }

        // Shows the Create Expense form
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = GetCurrentUserId();

            if (role != "Tenant" || userId is null)
                return RedirectToAction("Login", "Users");

            PopulateHousematesExceptCurrent();
            return View();
        }

        // Handles form submission for creating a new expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense expense, int[] shareWithUserIds)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return RedirectToAction("Login", "Users");

            if (!ModelState.IsValid)
            {
                PopulateHousematesExceptCurrent();
                return View(expense);
            }

            expense.PaidByUserId = userId.Value;
            expense.SharedWithUsers = _context.Users
                .Where(u => shareWithUserIds.Contains(u.Id))
                .ToList();

            _context.Expenses.Add(expense);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Shows the Settle Expense confirmation page (only payer can settle)
        public IActionResult Settle(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId is null) return RedirectToAction("Login", "Users");

            if (expense.PaidByUserId != userId.Value) return Forbid();

            return View(expense);
        }

        // Handles expense settlement (only payer can settle)
        [HttpPost, ActionName("Settle")]
        [ValidateAntiForgeryToken]
        public IActionResult SettleConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return RedirectToAction("Login", "Users");

            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();

            if (expense.PaidByUserId != userId.Value || expense.IsSettled) return Forbid();

            expense.IsSettled = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(MyExpenses));
        }

        // Shows the Delete Expense confirmation page (only payer can delete)
        // Allow payer to delete regardless of settlement status.
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId is null) return RedirectToAction("Login", "Users");

            if (expense.PaidByUserId != userId.Value) return Forbid();

            return View(expense); // Confirmation view
        }

        // Handles deletion of an expense (only payer can delete)
        // Allow payer to delete regardless of settlement status.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return RedirectToAction("Login", "Users");

            var expense = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null) return NotFound();

            if (expense.PaidByUserId != userId.Value) return Forbid();

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            // Deleting from current list → back to Index
            return RedirectToAction(nameof(Index));
        }
    }
}
