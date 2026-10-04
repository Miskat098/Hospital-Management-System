using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Staff Management Controller (Admin-only)
    /// GET/POST /api/staff, PUT/DELETE /api/staff/{id}
    /// </summary>
    [ApiController]
    [Route("api/staff")]
    // [Authorize(Roles = UserRoles.Admin)]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role)
        {
            var staff = await _staffService.GetAllAsync(role);
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            return staff == null
                ? NotFound(new { message = "Account not found." })
                : Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var created = await _staffService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] StaffCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var updated = await _staffService.UpdateAsync(id, dto);
                return updated == null
                    ? NotFound(new { message = "Account not found." })
                    : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _staffService.DeleteAsync(id);
                return deleted
                    ? Ok(new { success = true })
                    : NotFound(new { message = "Account not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}