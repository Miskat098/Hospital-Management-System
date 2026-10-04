using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Optional doctor who heads the department.</summary>
        public string? HeadDoctorId { get; set; }
        public Doctor? HeadDoctor { get; set; }

        /// <summary>"Active" | "Under Review" | "Inactive"</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}