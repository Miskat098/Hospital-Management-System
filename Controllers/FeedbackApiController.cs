using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class FeedbackApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public FeedbackApiController(ApplicationDbContext context) { _context = context; }

        [HttpGet("/api/feedbacks")]
        public async Task<IActionResult> List()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var query = _context.Feedbacks
                .Include(f => f.Patient).ThenInclude(p => p.User)
                .Include(f => f.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // all
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == sessionUserId);
                if (doctor == null) return Ok(new object[0]);
                query = query.Where(f => f.DoctorID == doctor.DoctorID);
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId);
                if (patient == null) return Ok(new object[0]);
                query = query.Where(f => f.PatientID == patient.PatientID);
            }
            else return Forbid();

            var list = await query.OrderByDescending(f => f.FeedbackID).ToListAsync();
            return Ok(list.Select(f => new { f.FeedbackID, Patient = f.Patient?.User?.Name, Doctor = f.Doctor?.User?.Name, f.Rating, f.Comment }));
        }

        [HttpPost("/api/feedbacks")]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Patient", StringComparison.OrdinalIgnoreCase)) return Forbid();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId);
            if (patient == null) return BadRequest(new { success = false, message = "Patient not found" });

            if (dto == null || dto.DoctorID <= 0) return BadRequest(new { success = false });

            var fb = new Feedback { PatientID = patient.PatientID, DoctorID = dto.DoctorID, Rating = dto.Rating, Comment = dto.Comment };
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, id = fb.FeedbackID });
        }

        public class CreateFeedbackDto { public int PatientID { get; set; } public int DoctorID { get; set; } public double Rating { get; set; } public string? Comment { get; set; } }
    }
}
