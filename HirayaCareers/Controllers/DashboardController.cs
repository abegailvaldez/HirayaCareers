using HirayaCareers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirayaCareers.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> JobSeeker()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.AvailableJobs = await _context.Jobs.CountAsync();

            ViewBag.MyApplications = await _context.JobApplications
                .CountAsync(a => a.UserId == userId.Value);

            ViewBag.ProfileStatus = "80%";

            return View();
        }

        public IActionResult Employer()
        {
            return View();
        }
    }
}