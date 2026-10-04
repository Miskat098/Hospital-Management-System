using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// APPOINTMENT — linked to BILLING (Invoice) and FEEDBACK per the ER
    /// diagram fixes. Doctors view/update their own appointments; patients
    /// search/book/cancel from the same table.
    /// </summary>
    public class Appointment
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty; // e.g. "APT-90205"

        [Required]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        [Required]
        public string DoctorId { get; set; } = string.Empty;
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        [MaxLength(20)]
        public string? RoomNumber { get; set; }

        /// <summary>Upcoming | InProgress | Completed | Cancelled</summary>
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Upcoming;

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Navigation — one appointment can have one invoice and one feedback entry
        public Invoice? Invoice { get; set; }
        public Feedback? Feedback { get; set; }
        public Prescription? Prescription { get; set; }
    }
}