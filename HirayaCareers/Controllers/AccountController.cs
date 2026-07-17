using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HirayaCareers.Data;
using HirayaCareers.Models;
using HirayaCareers.ViewModels;

namespace HirayaCareers.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET Register
        public IActionResult Register()
        {
            return View();
        }

        // POST Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = model.Password,
                    Role = model.Role
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // GET Login
        public IActionResult Login()
        {
            return View();
        }

        // POST Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "JobSeeker")
                    {
                        return RedirectToAction("Index", "Jobs");
                    }

                    if (user.Role == "Employer")
                    {
                        return RedirectToAction("Dashboard", "Employer");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}