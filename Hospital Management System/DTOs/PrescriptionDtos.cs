using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches the prescription-card layout rendered on
    /// wwwroot/pages/patient/prescriptions.html and
    /// wwwroot/pages/doctor/patients.html history entries.
    /// </summary>
    public class PrescriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime IssuedAt { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }

    public class PrescriptionItemDto
    {
        public string MedicineId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Matches the payload built by doctor/prescriptions.js
    /// initPrescriptionForm handler:
    /// { patientId, notes, items: [{ medicineId, dosage, frequency, duration }] }
    /// </summary>
    public class CreatePrescriptionDto
    {
        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [Required, MinLength(1, ErrorMessage = "A prescription needs at least one medicine.")]
        public List<CreatePrescriptionItemDto> Items { get; set; } = new();
    }

    public class CreatePrescriptionItemDto
    {
        [Required]
        public string MedicineId { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Dosage { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Frequency { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Duration { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
    }
}