using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class BillingApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BillingApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/billing/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var b = await _context.Billings.Include(bi => bi.Appointment).ThenInclude(a => a.Patient).FirstOrDefaultAsync(x => x.BillID == id);
            if (b == null) return NotFound();
            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var appt = b.Appointment;
                if (appt == null) return NotFound();
                if (appt.Patient == null || appt.Patient.UserID != sessionUserId) return Forbid();
            }
            return Ok(b);
        }

        [HttpPut("/api/billing/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBillingDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            var bill = await _context.Billings.FindAsync(id);
            if (bill==null) return NotFound();
            if (dto.Amount!=default) bill.Amount = dto.Amount;
            bill.PaymentStatus = dto.PaymentStatus ?? bill.PaymentStatus;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("/api/billing")]
        public async Task<IActionResult> Create([FromBody] CreateBillingDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            if (dto == null || dto.ApptID <= 0) return BadRequest(new { success = false });
            var appt = await _context.Appointments.FindAsync(dto.ApptID);
            if (appt == null) return BadRequest(new { success = false, message = "Appointment not found." });

            var bill = new Billing { ApptID = dto.ApptID, Amount = dto.Amount, PaymentStatus = dto.PaymentStatus ?? "Unpaid" };
            _context.Billings.Add(bill);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, billId = bill.BillID });
        }

        [HttpGet("/api/billing")]
        public async Task<IActionResult> List()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            var query = _context.Billings
                .Include(b => b.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.User)
                .Include(b => b.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // no filter
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == sessionUserId);
                if (patient == null) return Ok(new object[0]);
                query = query.Where(b => b.Appointment.PatientID == patient.PatientID);
            }
            else
            {
                return Forbid();
            }

            var list = await query.OrderByDescending(b => b.BillID).ToListAsync();
            return Ok(list.Select(b => new {
                b.BillID,
                ApptID = b.ApptID,
                Patient = b.Appointment?.Patient?.User?.Name,
                Amount = b.Amount,
                Status = b.PaymentStatus
            }));
        }

        public class CreateBillingDto { public int ApptID { get; set; } public decimal Amount { get; set; } public string? PaymentStatus { get; set; } }
    }
}
