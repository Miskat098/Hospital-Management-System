using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        // Standard clinic hours used to generate bookable slots.
        private static readonly TimeSpan[] StandardSlots =
        {
            new(9, 0, 0), new(9, 30, 0), new(10, 0, 0), new(10, 30, 0),
            new(11, 0, 0), new(11, 30, 0), new(14, 0, 0), new(14, 30, 0),
            new(15, 0, 0)
        };

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentDto>> GetAllAsync(AppointmentQueryDto query)
        {
            var q = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Department)
                .Include(a => a.Prescription)
                .Include(a => a.Invoice)
                .Include(a => a.Feedback)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.DoctorId))
                q = q.Where(a => a.DoctorId == query.DoctorId);

            if (!string.IsNullOrWhiteSpace(query.PatientId))
                q = q.Where(a => a.PatientId == query.PatientId);

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                Enum.TryParse<AppointmentStatus>(query.Status, true, out var status))
                q = q.Where(a => a.Status == status);

            if (query.Date.HasValue)
                q = q.Where(a => a.AppointmentDate.Date == query.Date.Value.Date);

            var results = await q
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return results.Select(ToDto).ToList();
        }

        public async Task<AppointmentDto?> GetByIdAsync(string id)
        {
            var appointment = await FindWithIncludesAsync(id);
            return appointment == null ? null : ToDto(appointment);
        }

        public async Task<AppointmentDto> BookAsync(string patientId, BookAppointmentDto dto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
            {
                throw new InvalidOperationException("Selected doctor could not be found.");
            }

            if (!TimeSpan.TryParse(ConvertTo24Hour(dto.Time), out var time))
            {
                throw new InvalidOperationException("Invalid time slot format.");
            }

            var conflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate.Date == dto.Date.Date &&
                a.AppointmentTime == time &&
                a.Status != AppointmentStatus.Cancelled);

            if (conflict)
            {
                throw new InvalidOperationException("This time slot has just been booked. Please choose another.");
            }

            var nextNumber = await _context.Appointments.CountAsync() + 90200;
            var appointment = new Appointment
            {
                Id = $"APT-{nextNumber}",
                PatientId = patientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.Date.Date,
                AppointmentTime = time,
                Reason = dto.Reason,
                ConsultationFee = doctor.ConsultationFee,
                RoomNumber = doctor.RoomNumber,
                Status = AppointmentStatus.Upcoming,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var saved = await FindWithIncludesAsync(appointment.Id);
            return ToDto(saved!);
        }

        public async Task<AppointmentDto?> RescheduleAsync(string id, RescheduleAppointmentDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return null;

            if (!TimeSpan.TryParse(ConvertTo24Hour(dto.NewTime), out var time))
            {
                throw new InvalidOperationException("Invalid time slot format.");
            }

            appointment.AppointmentDate = dto.NewDate.Date;
            appointment.AppointmentTime = time;
            appointment.Status = AppointmentStatus.Upcoming;

            await _context.SaveChangesAsync();

            var saved = await FindWithIncludesAsync(id);
            return ToDto(saved!);
        }

        public async Task<bool> CancelAsync(string id, string? reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason;
            appointment.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppointmentDto?> StartVisitAsync(string id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return null;

            appointment.Status = AppointmentStatus.InProgress;
            await _context.SaveChangesAsync();

            var saved = await FindWithIncludesAsync(id);
            return ToDto(saved!);
        }

        public async Task<AppointmentDto?> CompleteAsync(string id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return null;

            appointment.Status = AppointmentStatus.Completed;
            appointment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var saved = await FindWithIncludesAsync(id);
            return ToDto(saved!);
        }

        public async Task<DoctorAvailabilityDto> GetAvailabilityAsync(string doctorId, DateTime date)
        {
            var bookedTimes = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == date.Date &&
                            a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var slots = StandardSlots.Select(slot => new TimeSlotDto
            {
                Time = FormatTime(slot),
                IsAvailable = !bookedTimes.Contains(slot)
            }).ToList();

            return new DoctorAvailabilityDto
            {
                DoctorId = doctorId,
                Date = date.Date,
                Slots = slots
            };
        }

        // --------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------
        private Task<Appointment?> FindWithIncludesAsync(string id) =>
            _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Department)
                .Include(a => a.Prescription)
                .Include(a => a.Invoice)
                .Include(a => a.Feedback)
                .FirstOrDefaultAsync(a => a.Id == id);

        private static AppointmentDto ToDto(Appointment a) => new()
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient.User.FullName,
            DoctorId = a.DoctorId,
            DoctorName = $"Dr. {a.Doctor.User.FullName}",
            Department = a.Doctor.Department.Name,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = FormatTime(a.AppointmentTime),
            Reason = a.Reason,
            ConsultationFee = a.ConsultationFee,
            RoomNumber = a.RoomNumber,
            Status = a.Status.ToString(),
            HasPrescription = a.Prescription != null,
            HasInvoice = a.Invoice != null,
            HasFeedback = a.Feedback != null
        };

        private static string FormatTime(TimeSpan time)
        {
            var dt = DateTime.Today.Add(time);
            return dt.ToString("h:mm tt");
        }

        private static string ConvertTo24Hour(string time12h)
        {
            if (DateTime.TryParse(time12h, out var parsed))
            {
                return parsed.ToString("HH:mm:ss");
            }
            return time12h;
        }
    }
}