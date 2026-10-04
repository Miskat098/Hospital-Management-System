using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class DoctorsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/doctors")]
        public async Task<IActionResult> List()
        {
            // Allow authenticated users to view the list of doctors (Admin, Doctor, Patient)
            var uid = HttpContext.Session.GetInt32("UserID");
            if (!uid.HasValue) return Unauthorized();

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .OrderBy(d => d.User.Name)
                .ToListAsync();

            return Ok(doctors.Select(d => new {
                d.DoctorID,
                Name = d.User.Name,
                Email = d.User.Email,
                Phone = d.User.Phone,
                Specialization = d.Specialization,
                Dept = d.Department != null ? d.Department.DeptName : null
            }));
        }

        [HttpDelete("/api/doctors/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorID == id);
            if (doctor == null) return NotFound(new { success = false });
            _context.Doctors.Remove(doctor);
            _context.Users.Remove(doctor.User);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("/api/doctors/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _context.Doctors.Include(x => x.User).Include(x => x.Department).FirstOrDefaultAsync(x => x.DoctorID == id);
            if (d == null) return NotFound();
            return Ok(new {
                d.DoctorID,
                Name = d.User.Name,
                Email = d.User.Email,
                Phone = d.User.Phone,
                Specialization = d.Specialization,
                DeptID = d.DeptID
            });
        }

        [HttpPut("/api/doctors/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateDoctorDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorID == id);
            if (doctor == null) return NotFound(new { success = false });

            doctor.User.Name = dto.Name ?? doctor.User.Name;
            doctor.User.Email = dto.Email ?? doctor.User.Email;
            doctor.User.Phone = dto.Phone ?? doctor.User.Phone;
            doctor.Specialization = dto.Specialization ?? doctor.Specialization;
            if (dto.DeptID != 0) doctor.DeptID = dto.DeptID;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("/api/doctors")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Specialization))
                return BadRequest(new { success = false, message = "Name, Email and Specialization are required." });

            bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists) return Conflict(new { success = false, message = "Email already registered." });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "Doctor",
                PasswordHash = Helpers.PasswordHelper.HashPassword(dto.Password ?? "changeme123")
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                UserID = user.UserID,
                Specialization = dto.Specialization,
                DeptID = dto.DeptID
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, userId = user.UserID, doctorId = doctor.DoctorID });
        }

        public class CreateDoctorDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string? Phone { get; set; }
            public string Specialization { get; set; }
            public int DeptID { get; set; }
            public string? Password { get; set; }
        }
    }
}
