using HirayaCareers.Data;
using HirayaCareers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirayaCareers.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ApplicationsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            ViewBag.JobTitle = job.Title;
            ViewBag.JobId = job.JobId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            int jobId,
            string coverLetter,
            IFormFile resumeFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null ||
                HttpContext.Session.GetString("Role") != "JobSeeker")
            {
                return RedirectToAction("Login", "Account");
            }

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a =>
                    a.JobId == jobId &&
                    a.UserId == userId.Value);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] =
                    "You have already applied for this job.";

                return RedirectToAction(nameof(MyApplications));
            }

            if (resumeFile == null || resumeFile.Length == 0)
            {
                ModelState.AddModelError(
                    "resumeFile",
                    "Please upload your resume.");

                var job = await _context.Jobs.FindAsync(jobId);

                ViewBag.JobId = jobId;
                ViewBag.JobTitle = job?.Title;

                return View();
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "resumeFile",
                    "Only PDF, DOC, and DOCX files are allowed.");

                var job = await _context.Jobs.FindAsync(jobId);

                ViewBag.JobId = jobId;
                ViewBag.JobTitle = job?.Title;

                return View();
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (resumeFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    "resumeFile",
                    "The resume must not exceed 5 MB.");

                var job = await _context.Jobs.FindAsync(jobId);

                ViewBag.JobId = jobId;
                ViewBag.JobTitle = job?.Title;

                return View();
            }

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "resumes");

            Directory.CreateDirectory(uploadsFolder);

            var safeFileName =
                $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(
                uploadsFolder,
                safeFileName);

            await using (var stream = new FileStream(
                fullPath,
                FileMode.Create))
            {
                await resumeFile.CopyToAsync(stream);
            }

            var application = new JobApplication
            {
                JobId = jobId,
                UserId = userId.Value,
                CoverLetter = coverLetter,
                ResumePath = $"/uploads/resumes/{safeFileName}",
                Status = "Pending",
                AppliedAt = DateTime.Now
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Application submitted successfully.";

            return RedirectToAction(nameof(MyApplications));
        }

        public async Task<IActionResult> MyApplications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.UserId == userId.Value)
                .ToListAsync();

            return View(applications);
        }
    }
}