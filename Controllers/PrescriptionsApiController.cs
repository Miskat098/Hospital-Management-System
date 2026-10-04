using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class PrescriptionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("/api/prescriptions/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var p = await _context.Prescriptions.Include(x=>x.Appointment).ThenInclude(a=>a.Patient).ThenInclude(pt=>pt.User).Include(x=>x.Appointment).ThenInclude(a=>a.Doctor).ThenInclude(d=>d.User).FirstOrDefaultAsync(x=>x.PrescID==id);
            if (p==null) return NotFound();

            // Admin can view any. Doctor can view prescriptions for their appointments. Patient can view own prescriptions only.
            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == p.Appointment.DoctorID);
                if (doc == null || doc.UserID != sessionUserId) return Forbid();
            }
            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var pat = await _context.Patients.FirstOrDefaultAsync(pt => pt.PatientID == p.Appointment.PatientID);
                if (pat == null || pat.UserID != sessionUserId) return Forbid();
            }

            return Ok(new { p.PrescID, p.ApptID, Notes = p.Notes, File = p.FileUpload });
        }

        [HttpPut("/api/prescriptions/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePrescDto dto)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var presc = await _context.Prescriptions.Include(p=>p.Appointment).FirstOrDefaultAsync(p=>p.PrescID==id);
            if (presc==null) return NotFound();

            // Only Doctor of the appointment or Admin can update
            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == presc.Appointment.DoctorID);
                if (doctor == null || doctor.UserID != sessionUserId) return Forbid();
            }
            else if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            presc.Notes = dto.Notes ?? presc.Notes;
            presc.FileUpload = dto.FileUpload ?? presc.FileUpload;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        [HttpPost("/api/prescriptions")]
        public async Task<IActionResult> Create([FromBody] CreatePrescDto dto)
        {
            if (dto == null || dto.ApptID <= 0 || string.IsNullOrWhiteSpace(dto.Notes))
                return BadRequest(new { success = false, message = "ApptID and Notes are required." });

            var appt = await _context.Appointments.FindAsync(dto.ApptID);
            if (appt == null) return BadRequest(new { success = false, message = "Appointment not found." });

            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            // Only Doctors (or Admin) can create prescriptions. If Doctor, appointment must belong to them.
            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == appt.DoctorID);
                if (doctor == null || doctor.UserID != sessionUserId) return Forbid();
            }
            else if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var presc = new Prescription { ApptID = dto.ApptID, Notes = dto.Notes, FileUpload = dto.FileUpload };
            _context.Prescriptions.Add(presc);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, prescId = presc.PrescID });
        }

        [HttpGet("/api/prescriptions")]
        public async Task<IActionResult> List()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var query = _context.Prescriptions
                .Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(pt => pt.User)
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // no filter
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == sessionUserId);
                if (doctor == null) return Ok(new object[0]);
                query = query.Where(p => p.Appointment.DoctorID == doctor.DoctorID);
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId);
                if (patient == null) return Ok(new object[0]);
                query = query.Where(p => p.Appointment.PatientID == patient.PatientID);
            }

            var list = await query.OrderByDescending(p => p.PrescID).ToListAsync();
            return Ok(list.Select(p => new {
                p.PrescID,
                ApptID = p.ApptID,
                Patient = p.Appointment?.Patient?.User?.Name,
                Doctor = p.Appointment?.Doctor?.User?.Name,
                Notes = p.Notes,
                File = p.FileUpload
            }));
        }

        public class CreatePrescDto { public int ApptID { get; set; } public string Notes { get; set; } public string? FileUpload { get; set; } }
    }
}
