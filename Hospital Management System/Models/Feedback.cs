using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// FEEDBACK / RATING — one per Appointment (1-to-1), matching the ER
    /// diagram's APPOINTMENT -> FEEDBACK link. Submitted by patients only.
    /// </summary>
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AppointmentId { get; set; } = string.Empty;
        public Appointment Appointment { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}