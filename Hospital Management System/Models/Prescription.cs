using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// PRESCRIPTION — written by a Doctor for a Patient, linked to a
    /// specific Appointment, and composed of one or more PRESCRIPTION_ITEMS
    /// (see PrescriptionItem.cs) per the ER diagram fixes.
    /// </summary>
    public class Prescription
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty; // e.g. "RX-20261921"

        [Required]
        public string AppointmentId { get; set; } = string.Empty;
        public Appointment Appointment { get; set; } = null!;

        [Required]
        public string DoctorId { get; set; } = string.Empty;
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}