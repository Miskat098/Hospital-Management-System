using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class DepartmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DepartmentsApiController(ApplicationDbContext context) { _context = context; }

        [HttpGet("/api/departments")]
        public async Task<IActionResult> List()
        {
            var uid = HttpContext.Session.GetInt32("UserID");
            if (!uid.HasValue) return Forbid();
            return Ok(await _context.Departments.OrderBy(d => d.DeptName).ToListAsync());
        }

        [HttpPost("/api/departments")]
        public async Task<IActionResult> Create([FromBody] Department dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.DeptName)) return BadRequest(new { success = false });
            _context.Departments.Add(dto);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, id = dto.DeptID });
        }

        [HttpDelete("/api/departments/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("/api/departments/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        [HttpPut("/api/departments/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            dept.DeptName = dto.DeptName ?? dept.DeptName;
            dept.Description = dto.Description ?? dept.Description;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
