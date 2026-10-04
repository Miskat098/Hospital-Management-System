using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentDto>> GetAllAsync(AppointmentQueryDto query);
        Task<AppointmentDto?> GetByIdAsync(string id);

        /// <summary>Books a new appointment on behalf of the currently
        /// authenticated patient.</summary>
        Task<AppointmentDto> BookAsync(string patientId, BookAppointmentDto dto);

        Task<AppointmentDto?> RescheduleAsync(string id, RescheduleAppointmentDto dto);
        Task<bool> CancelAsync(string id, string? reason);

        /// <summary>Doctor marks an appointment as started/in-progress.</summary>
        Task<AppointmentDto?> StartVisitAsync(string id);

        /// <summary>Doctor marks an appointment as completed.</summary>
        Task<AppointmentDto?> CompleteAsync(string id);

        /// <summary>Returns bookable time slots for a doctor on a given date,
        /// powering the slot-picker on patient/appointments.html.</summary>
        Task<DoctorAvailabilityDto> GetAvailabilityAsync(string doctorId, DateTime date);
    }
}