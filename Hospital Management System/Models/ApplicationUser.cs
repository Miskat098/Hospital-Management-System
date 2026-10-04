using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// Base identity user for every role in the system (Admin, Doctor,
    /// Patient, Staff). Matches the USER entity in the ER diagram, with
    /// DOCTOR and PATIENT as 1-to-1 role-specific extensions below.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        /// <summary>"Male" | "Female" | "Other"</summary>
        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Convenience denormalized role label ("Admin" | "Doctor" |
        /// "Patient" | "Staff") kept in sync with ASP.NET Identity's
        /// AspNetUserRoles table, so DTOs can return a single role
        /// string the same way the frontend's mock users do.
        /// </summary>
        public string PrimaryRole { get; set; } = "Patient";

        /// <summary>"Active" | "Pending" | "Suspended"</summary>
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation — populated only for users whose PrimaryRole is Doctor/Patient
        public Doctor? DoctorProfile { get; set; }
        public Patient? PatientProfile { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}