using Microsoft.AspNetCore.Mvc;
using SharedHousingApp.Models;
using SharedHousingApp.Helpers;
using SharedHousingApp.Data;
using System.Linq;

namespace SharedHousingApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // Shows the Register form
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Handles registration form submission (hashes password before saving)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = Hasher.Hash(user.Password);
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // Shows the Login form
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handles login form submission (validates credentials and sets session)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User loginUser)
        {
            // Ignore unrelated required fields on the User model for this screen
            ModelState.Remove("Name");
            ModelState.Remove("Role"); // remove if Role isn't required

            // Only Email + Password need to be valid/present
            if (!ModelState.IsValid)
                return View(loginUser);

            var email = (loginUser.Email ?? "").Trim().ToLowerInvariant();
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email);

            if (user != null && Hasher.Verify(loginUser.Password, user.Password))
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role ?? "");
                HttpContext.Session.SetString("UserName", user.Name ?? "");
                return RedirectToAction("Index", "Home");
            }

            // Single friendly message — no mention of Name
            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            return View(loginUser);
        }

        // Logs the user out by clearing the session
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
