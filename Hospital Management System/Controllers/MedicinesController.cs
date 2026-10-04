using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Medicines Management Controller
    /// GET/POST /api/medicines, PUT/DELETE /api/medicines/{id}
    /// GET /api/medicines/options (For prescription drug dropdowns)
    /// </summary>
    [ApiController]
    [Route("api/medicines")]
    [Authorize]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Doctor)]
        public async Task<IActionResult> GetAll()
        {
            var medicines = await _medicineService.GetAllAsync();
            return Ok(medicines);
        }

        [HttpGet("options")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Doctor)]
        public async Task<IActionResult> GetOptions()
        {
            var options = await _medicineService.GetOptionsAsync();
            return Ok(options);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Doctor)]
        public async Task<IActionResult> GetById(string id)
        {
            var medicine = await _medicineService.GetByIdAsync(id);
            return medicine == null
                ? NotFound(new { message = "Medicine not found." })
                : Ok(medicine);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] MedicineCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var created = await _medicineService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(string id, [FromBody] MedicineCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var updated = await _medicineService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = "Medicine not found." })
                : Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _medicineService.DeleteAsync(id);
                return deleted
                    ? Ok(new { success = true })
                    : NotFound(new { message = "Medicine not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}