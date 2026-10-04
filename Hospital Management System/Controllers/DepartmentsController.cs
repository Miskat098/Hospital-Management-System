using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Departments Management Controller
    /// GET/POST /api/departments, PUT/DELETE /api/departments/{id}
    /// </summary>
    [ApiController]
    [Route("api/departments")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [AllowAnonymous] // ডিপার্টমেন্টের তালিকা যেকোনো সাধারণ ইউজার বা পেজে দেখানোর জন্য ওপেন রাখা হলো
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            return department == null
                ? NotFound(new { message = "Department not found." })
                : Ok(department);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var updated = await _departmentService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = "Department not found." })
                : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _departmentService.DeleteAsync(id);
                return deleted
                    ? Ok(new { success = true })
                    : NotFound(new { message = "Department not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}