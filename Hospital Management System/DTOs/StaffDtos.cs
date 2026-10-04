using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches each row rendered in
    /// wwwroot/pages/admin/staff-management.html, covering all three
    /// manageable roles (Doctor, Patient, Staff) in one unified shape.
    /// </summary>
    public class StaffDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Doctor | Patient | Staff
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Department { get; set; } // Doctors only
        public string Status { get; set; } = "Active";
        public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// Matches the payload built by admin/staff.js from #staffForm:
    /// { role, firstName, lastName, email, phone, department, status }
    /// </summary>
    public class StaffCreateUpdateDto
    {
        [Required]
        public string Role { get; set; } = string.Empty; // Doctor | Patient | Staff

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        /// <summary>Department name or Id — only required when Role == "Doctor".</summary>
        public string? Department { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}