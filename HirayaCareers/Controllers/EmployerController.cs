using HirayaCareers.Data;
using HirayaCareers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirayaCareers.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsEmployer()
        {
            return HttpContext.Session.GetString("Role") == "Employer";
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.JobsPosted = await _context.Jobs
                .CountAsync(j => j.EmployerId == userId.Value);

            ViewBag.Applications = await _context.JobApplications
                .CountAsync(a =>
                    a.Job != null &&
                    a.Job.EmployerId == userId.Value);

            ViewBag.OpenPositions = await _context.Jobs
                .CountAsync(j => j.EmployerId == userId.Value);

            return View();
        }

        public IActionResult PostJob()
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(Job job)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                job.EmployerId = userId.Value;
                job.DatePosted = DateTime.Now;

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Job posted successfully.";

                return RedirectToAction("ManageJobs");
            }

            return View(job);
        }

        public async Task<IActionResult> ManageJobs()
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == userId.Value)
                .OrderByDescending(j => j.DatePosted)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> EditJob(int id)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == userId.Value);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(Job job)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingJob = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == job.JobId &&
                    j.EmployerId == userId.Value);

            if (existingJob == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(job);
            }

            existingJob.Title = job.Title;
            existingJob.CompanyName = job.CompanyName;
            existingJob.Location = job.Location;
            existingJob.EmploymentType = job.EmploymentType;
            existingJob.Description = job.Description;
            existingJob.Requirements = job.Requirements;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Job updated successfully.";
            return RedirectToAction(nameof(ManageJobs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == userId.Value);

            if (job == null)
            {
                return NotFound();
            }

            var applications = await _context.JobApplications
                .Where(a => a.JobId == job.JobId)
                .ToListAsync();

            if (applications.Any())
            {
                _context.JobApplications.RemoveRange(applications);
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Job and its related applications were deleted successfully.";

            return RedirectToAction(nameof(ManageJobs));
        }

        public async Task<IActionResult> Applicants(int id)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ownsJob = await _context.Jobs
                .AnyAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == userId.Value);

            if (!ownsJob)
            {
                return NotFound();
            }

            var applications = await _context.JobApplications
                .Include(a => a.User)
                .Include(a => a.Job)
                .Where(a => a.JobId == id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!IsEmployer())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var allowedStatuses = new[] { "Pending", "Accepted", "Rejected" };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.JobApplicationId == id &&
                    a.Job != null &&
                    a.Job.EmployerId == userId.Value);

            if (application == null)
            {
                return NotFound();
            }

            application.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] =
                $"Application status updated to {status}.";
            return RedirectToAction(
                nameof(Applicants),
                new { id = application.JobId });
        }
    }
}