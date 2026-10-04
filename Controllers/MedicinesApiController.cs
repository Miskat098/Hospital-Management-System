using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    public class MedicinesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MedicinesApiController(ApplicationDbContext context) { _context = context; }

        [HttpGet("/api/medicines")]
        public async Task<IActionResult> List()
        {
            var uid = HttpContext.Session.GetInt32("UserID");
            if (!uid.HasValue) return Forbid();
            return Ok(await _context.Medicines.OrderBy(m => m.Name).ToListAsync());
        }

        [HttpPost("/api/medicines")]
        public async Task<IActionResult> Create([FromBody] Medicine dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { success = false });
            _context.Medicines.Add(dto);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, id = dto.MedID });
        }

        [HttpDelete("/api/medicines/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            var med = await _context.Medicines.FindAsync(id);
            if (med == null) return NotFound();
            _context.Medicines.Remove(med);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("/api/medicines/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var med = await _context.Medicines.FindAsync(id);
            if (med == null) return NotFound();
            return Ok(med);
        }

        [HttpPut("/api/medicines/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Medicine dto)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
            var med = await _context.Medicines.FindAsync(id);
            if (med == null) return NotFound();
            med.Name = dto.Name ?? med.Name;
            med.Stock = dto.Stock;
            med.Price = dto.Price;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
