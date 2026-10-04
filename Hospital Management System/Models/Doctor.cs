using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// DOCTOR — 1-to-1 role extension of ApplicationUser (USER -> DOCTOR
    /// inheritance from the ER diagram).
    /// </summary>
    public class Doctor
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; } = string.Empty; // shares PK with ApplicationUser.Id

        public ApplicationUser User { get; set; } = null!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        [MaxLength(150)]
        public string Specialization { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; } // e.g. BMDC-44210

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        [MaxLength(20)]
        public string? RoomNumber { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        public bool IsAvailable { get; set; } = true;

        // Navigation
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}