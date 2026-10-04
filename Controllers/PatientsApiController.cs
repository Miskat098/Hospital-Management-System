using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class PatientsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/patients/forcurrent")]
        public async Task<IActionResult> ForCurrent()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            if (!sessionUserId.HasValue) return Unauthorized();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var all = await _context.Patients.Include(p => p.User).OrderBy(p => p.User.Name).ToListAsync();
                return Ok(all.Select(p => new {
                    p.PatientID,
                    Name = p.User.Name,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    DOB = p.DOB,
                    Gender = p.Gender,
                    BloodGroup = p.BloodGroup
                }));
            }

            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                // Find doctor record for current user
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == sessionUserId.Value);
                if (doctor == null) return Ok(new object[0]);

                // Get patient IDs from appointments assigned to this doctor
                var patientIds = await _context.Appointments
                    .Where(a => a.DoctorID == doctor.DoctorID)
                    .Select(a => a.PatientID)
                    .Distinct()
                    .ToListAsync();

                if (!patientIds.Any()) return Ok(new object[0]);

                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Where(p => patientIds.Contains(p.PatientID))
                    .OrderBy(p => p.User.Name)
                    .ToListAsync();

                return Ok(patients.Select(p => new {
                    p.PatientID,
                    Name = p.User.Name,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    DOB = p.DOB,
                    Gender = p.Gender,
                    BloodGroup = p.BloodGroup
                }));
            }

            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.UserID == sessionUserId.Value);
                if (patient == null) return Ok(new object[0]);
                return Ok(new[] { new {
                    patient.PatientID,
                    Name = patient.User.Name,
                    Email = patient.User.Email,
                    Phone = patient.User.Phone,
                    DOB = patient.DOB,
                    Gender = patient.Gender,
                    BloodGroup = patient.BloodGroup
                } });
            }

            return Forbid();
        }

        [HttpGet("/api/patients")]
        public async Task<IActionResult> List()
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var patients = await _context.Patients
                .Include(p => p.User)
                .OrderBy(p => p.User.Name)
                .ToListAsync();

            return Ok(patients.Select(p => new {
                p.PatientID,
                Name = p.User.Name,
                Email = p.User.Email,
                Phone = p.User.Phone,
                DOB = p.DOB,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup
            }));
        }

        [HttpDelete("/api/patients/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientID == id);
            if (patient == null) return NotFound(new { success = false });
            _context.Patients.Remove(patient);
            _context.Users.Remove(patient.User);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("/api/patients/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var p = await _context.Patients.Include(x => x.User).FirstOrDefaultAsync(x => x.PatientID == id);
            if (p == null) return NotFound();

            // Authorization rules:
            // - Admin can view any
            // - Patient can view only own record
            // - Doctor can view only patients assigned to them via at least one appointment
            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                if (!sessionUserId.HasValue || p.User.UserID != sessionUserId.Value)
                    return Forbid();
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                if (!sessionUserId.HasValue) return Forbid();
                // find doctor record for current user
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == sessionUserId.Value);
                if (doctor == null) return Forbid();

                // check if there's at least one appointment linking this doctor to the patient
                var hasAppt = await _context.Appointments.AnyAsync(a => a.DoctorID == doctor.DoctorID && a.PatientID == p.PatientID);
                if (!hasAppt)
                    return Forbid();
            }

            return Ok(new {
                p.PatientID,
                Name = p.User.Name,
                Email = p.User.Email,
                Phone = p.User.Phone,
                DOB = p.DOB,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup
            });
        }

        [HttpPut("/api/patients/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePatientDto dto)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientID == id);
            if (patient == null) return NotFound(new { success = false });

            // Only Admin or Doctor can update patient via this admin UI
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            // update user
            patient.User.Name = dto.Name ?? patient.User.Name;
            patient.User.Email = dto.Email ?? patient.User.Email;
            patient.User.Phone = dto.Phone ?? patient.User.Phone;

            // update patient fields
            if (dto.DOB != default) patient.DOB = dto.DOB;
            patient.Gender = dto.Gender ?? patient.Gender;
            patient.BloodGroup = dto.BloodGroup ?? patient.BloodGroup;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("/api/patients")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { success = false, message = "Name and Email are required." });

            bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists) return Conflict(new { success = false, message = "Email already registered." });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "Patient",
                PasswordHash = Helpers.PasswordHelper.HashPassword(dto.Password ?? "changeme123")
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var patient = new Patient
            {
                UserID = user.UserID,
                DOB = dto.DOB,
                Gender = dto.Gender,
                BloodGroup = dto.BloodGroup
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, userId = user.UserID, patientId = patient.PatientID });
        }

        public class CreatePatientDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string? Phone { get; set; }
            public DateTime DOB { get; set; }
            public string? Gender { get; set; }
            public string? BloodGroup { get; set; }
            public string? Password { get; set; }
        }
    }
}
