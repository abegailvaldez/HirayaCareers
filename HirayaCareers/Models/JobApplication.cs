using System.ComponentModel.DataAnnotations;

namespace HirayaCareers.Models
{
    public class JobApplication
    {
        public int JobApplicationId { get; set; }

        public int JobId { get; set; }
        public Job? Job { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string CoverLetter { get; set; } = string.Empty;
        public string? ResumePath { get; set; }
        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}