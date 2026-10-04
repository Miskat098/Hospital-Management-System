using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PrescriptionDto>> GetForPatientAsync(string patientId)
        {
            var prescriptions = await BaseQuery()
                .Where(rx => rx.PatientId == patientId)
                .OrderByDescending(rx => rx.IssuedAt)
                .ToListAsync();

            return prescriptions.Select(ToDto).ToList();
        }

        public async Task<List<PrescriptionDto>> GetForDoctorAsync(string doctorId)
        {
            var prescriptions = await BaseQuery()
                .Where(rx => rx.DoctorId == doctorId)
                .OrderByDescending(rx => rx.IssuedAt)
                .ToListAsync();

            return prescriptions.Select(ToDto).ToList();
        }

        public async Task<PrescriptionDto?> GetByIdAsync(string id)
        {
            var prescription = await BaseQuery().FirstOrDefaultAsync(rx => rx.Id == id);
            return prescription == null ? null : ToDto(prescription);
        }

        public async Task<PrescriptionDto> CreateAsync(string doctorId, CreatePrescriptionDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }
            if (appointment.DoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("You can only write prescriptions for your own appointments.");
            }

            var medicineIds = dto.Items.Select(i => i.MedicineId).ToList();
            var validMedicineCount = await _context.Medicines.CountAsync(m => medicineIds.Contains(m.Id));
            if (validMedicineCount != medicineIds.Distinct().Count())
            {
                throw new InvalidOperationException("One or more selected medicines could not be found.");
            }

            var nextNumber = await _context.Prescriptions.CountAsync() + 20261900;
            var prescription = new Prescription
            {
                Id = $"RX-{nextNumber}",
                AppointmentId = dto.AppointmentId,
                DoctorId = doctorId,
                PatientId = dto.PatientId,
                Notes = dto.Notes,
                FollowUpDate = dto.FollowUpDate,
                IssuedAt = DateTime.UtcNow,
                Items = dto.Items.Select(i => new PrescriptionItem
                {
                    MedicineId = i.MedicineId,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Prescriptions.Add(prescription);

            // Writing a prescription implicitly completes the visit.
            appointment.Status = AppointmentStatus.Completed;
            appointment.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var saved = await BaseQuery().FirstAsync(rx => rx.Id == prescription.Id);
            return ToDto(saved);
        }

        // --------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------
        private IQueryable<Prescription> BaseQuery() =>
            _context.Prescriptions
                .Include(rx => rx.Doctor).ThenInclude(d => d.User)
                .Include(rx => rx.Doctor).ThenInclude(d => d.Department)
                .Include(rx => rx.Patient).ThenInclude(p => p.User)
                .Include(rx => rx.Items).ThenInclude(i => i.Medicine);

        private static PrescriptionDto ToDto(Prescription rx) => new()
        {
            Id = rx.Id,
            AppointmentId = rx.AppointmentId,
            DoctorId = rx.DoctorId,
            DoctorName = $"Dr. {rx.Doctor.User.FullName}",
            Department = rx.Doctor.Department.Name,
            PatientId = rx.PatientId,
            PatientName = rx.Patient.User.FullName,
            Notes = rx.Notes,
            FollowUpDate = rx.FollowUpDate,
            IssuedAt = rx.IssuedAt,
            Items = rx.Items.Select(i => new PrescriptionItemDto
            {
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.Name,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}