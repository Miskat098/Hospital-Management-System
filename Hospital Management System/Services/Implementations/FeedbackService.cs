using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FeedbackDto>> GetForPatientAsync(string patientId)
        {
            var entries = await BaseQuery()
                .AsNoTracking()
                .Where(f => f.PatientId == patientId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            return entries.Select(ToDto).ToList();
        }

        public async Task<FeedbackDto> SubmitAsync(string patientId, SubmitFeedbackDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }
            if (appointment.PatientId != patientId)
            {
                throw new UnauthorizedAccessException("You can only leave feedback for your own appointments.");
            }
            if (appointment.Status != AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Feedback can only be submitted for completed visits.");
            }

            var alreadyExists = await _context.FeedbackEntries
                .AnyAsync(f => f.AppointmentId == dto.AppointmentId);
            if (alreadyExists)
            {
                throw new InvalidOperationException("Feedback has already been submitted for this visit.");
            }

            var feedback = new Feedback
            {
                AppointmentId = dto.AppointmentId,
                PatientId = patientId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsAnonymous = dto.Anonymous,
                SubmittedAt = DateTime.UtcNow
            };

            _context.FeedbackEntries.Add(feedback);
            await _context.SaveChangesAsync();

            // ডাক্তারের গড় রেটিং (Rating) আপডেট করা
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor != null)
            {
                var newCount = doctor.ReviewCount + 1;
                var newAverage = ((doctor.Rating * doctor.ReviewCount) + dto.Rating) / newCount;
                doctor.Rating = Math.Round(newAverage, 2);
                doctor.ReviewCount = newCount;
                await _context.SaveChangesAsync();
            }

            var saved = await BaseQuery().FirstAsync(f => f.Id == feedback.Id);
            return ToDto(saved);
        }

        // --------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------
        private IQueryable<Feedback> BaseQuery() =>
            _context.FeedbackEntries
                .Include(f => f.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(f => f.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.Department);

        private static FeedbackDto ToDto(Feedback f) => new()
        {
            Id = f.Id,
            AppointmentId = f.AppointmentId,
            DoctorName = f.Appointment?.Doctor?.User?.FullName != null
                ? $"Dr. {f.Appointment.Doctor.User.FullName}"
                : "Unknown Doctor",
            Department = f.Appointment?.Doctor?.Department?.Name ?? "N/A",
            Rating = f.Rating,
            Comment = f.Comment,
            IsAnonymous = f.IsAnonymous,
            SubmittedAt = f.SubmittedAt
        };
    }
}