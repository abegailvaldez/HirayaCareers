using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HirayaCareers.Data;

namespace HirayaCareers.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, string location)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var jobs = _context.Jobs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                jobs = jobs.Where(j =>
                    j.Title.Contains(searchTerm) ||
                    j.CompanyName.Contains(searchTerm) ||
                    j.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                jobs = jobs.Where(j => j.Location.Contains(location));
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Location = location;

            return View(await jobs.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }
    }
}