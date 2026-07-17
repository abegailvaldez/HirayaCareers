using System.ComponentModel.DataAnnotations;

namespace HirayaCareers.Models
{
    public class Job
    {
        public int JobId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        public DateTime DatePosted { get; set; } = DateTime.Now;

        public int EmployerId { get; set; }
        public User? Employer { get; set; }
    }
}