using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// PATIENT — 1-to-1 role extension of ApplicationUser (USER -> PATIENT
    /// inheritance from the ER diagram). Holds self-service profile fields
    /// (medical info) editable from pages/patient/profile.html.
    /// </summary>
    public class Patient
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; } = string.Empty; // shares PK with ApplicationUser.Id

        public ApplicationUser User { get; set; } = null!;

        [MaxLength(5)]
        public string? BloodGroup { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-

        public int? HeightCm { get; set; }

        [MaxLength(500)]
        public string? Allergies { get; set; } // comma-separated, matches mAllergies field

        [MaxLength(500)]
        public string? ChronicConditions { get; set; }

        [MaxLength(1000)]
        public string? CurrentMedications { get; set; }

        [MaxLength(200)]
        public string? EmergencyContact { get; set; }

        [MaxLength(30)]
        public string? NationalId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Feedback> FeedbackEntries { get; set; } = new List<Feedback>();
    }
}