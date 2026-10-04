using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/appointments")]
        public async Task<IActionResult> List()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            if (!sessionUserId.HasValue) return Unauthorized();

            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // admin can see all
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == sessionUserId.Value);
                if (doctor == null) return Ok(new object[0]);
                query = query.Where(a => a.DoctorID == doctor.DoctorID);
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId.Value);
                if (patient == null) return Ok(new object[0]);
                query = query.Where(a => a.PatientID == patient.PatientID);
            }
            else
            {
                return Forbid();
            }

            var list = await query.OrderByDescending(a => a.AppointmentDateTime).ToListAsync();
            return Ok(list.Select(a => new {
                a.ApptID,
                Patient = a.Patient?.User?.Name,
                Doctor = a.Doctor?.User?.Name,
                a.AppointmentDateTime,
                a.Status
            }));
        }

        [HttpPost("/api/appointments")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized(new { success = false, message = "Not authenticated." });

            int patientId;
            if (string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                // session stores UserID (Users.UserID). We need the Patient.PatientID linked to this user.
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId);
                if (patient == null) return BadRequest(new { success = false, message = "Patient profile not found." });
                patientId = patient.PatientID;
            }
            else if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!dto.PatientID.HasValue) return BadRequest(new { success = false, message = "PatientID is required." });
                patientId = dto.PatientID.Value;
            }
            else
            {
                // Other roles (e.g., Doctor) cannot create appointments for patients via this API
                return Forbid();
            }

            var doctor = await _context.Doctors.FindAsync(dto.DoctorID);
            if (doctor == null) return BadRequest(new { success = false, message = "Doctor not found." });

            var appt = new Appointment
            {
                PatientID = patientId,
                DoctorID = dto.DoctorID,
                AppointmentDateTime = dto.AppointmentDateTime,
                Status = dto.Status ?? "Scheduled"
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, apptId = appt.ApptID });
        }

        [HttpGet("/api/appointments/forcurrent")]
        public async Task<IActionResult> ForCurrent()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("UserRole");

            if (!userId.HasValue) return Unauthorized();

            if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId.Value);
                if (doctor == null) return Ok(new object[0]);

                var appts = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Where(a => a.DoctorID == doctor.DoctorID)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToListAsync();

                return Ok(appts.Select(a => new { a.ApptID, PatientName = a.Patient.User.Name, a.AppointmentDateTime, a.Status }));
            }
            else // patient or admin
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId.Value);
                if (patient == null) return Ok(new object[0]);

                var appts = await _context.Appointments
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.PatientID == patient.PatientID)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToListAsync();

                return Ok(appts.Select(a => new { a.ApptID, DoctorName = a.Doctor.User.Name, a.AppointmentDateTime, a.Status }));
            }
        }

        public class CreateAppointmentDto
        {
            public int? PatientID { get; set; }
            public int DoctorID { get; set; }
            public DateTime AppointmentDateTime { get; set; }
            public string? Status { get; set; }
        }
    }
}
