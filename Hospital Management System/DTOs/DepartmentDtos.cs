using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches each department-card's rendered fields in
    /// wwwroot/pages/admin/department-management.html.
    /// </summary>
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? HeadDoctorId { get; set; }
        public string? HeadDoctorName { get; set; }
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public string Status { get; set; } = "Active";
    }

    /// <summary>
    /// Matches the payload built by admin/departments.js from
    /// #departmentForm: { name, description, headDoctorId, status }
    /// </summary>
    public class DepartmentCreateUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? HeadDoctorId { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}