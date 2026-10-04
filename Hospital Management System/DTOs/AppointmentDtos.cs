using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches the appointment-card / timeline-card fields rendered across
    /// wwwroot/pages/doctor/appointments.html, doctor/dashboard.html, and
    /// patient/appointments.html.
    /// </summary>
    public class AppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty; // "10:30 AM"
        public string? Reason { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? RoomNumber { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming | InProgress | Completed | Cancelled
        public bool HasPrescription { get; set; }
        public bool HasInvoice { get; set; }
        public bool HasFeedback { get; set; }
    }

    /// <summary>
    /// Matches the payload built by patient/appointments.js
    /// confirmBookingBtn handler: { doctorId, date, time, reason }
    /// </summary>
    public class BookAppointmentDto
    {
        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; } = string.Empty; // "10:30 AM"

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Matches the payload built by doctor/appointments.js and
    /// patient/appointments.js reschedule forms:
    /// { newDate, newTime, reason }
    /// </summary>
    public class RescheduleAppointmentDto
    {
        [Required]
        public DateTime NewDate { get; set; }

        [Required]
        public string NewTime { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>Query filters accepted by GET /api/appointments.</summary>
    public class AppointmentQueryDto
    {
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public string? Status { get; set; }
        public DateTime? Date { get; set; }
    }

    /// <summary>Available time slots for a given doctor/date, used by the
    /// slot-picker on patient/appointments.html.</summary>
    public class DoctorAvailabilityDto
    {
        public string DoctorId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<TimeSlotDto> Slots { get; set; } = new();
    }

    public class TimeSlotDto
    {
        public string Time { get; set; } = string.Empty; // "10:30 AM"
        public bool IsAvailable { get; set; }
    }
}