using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Prescriptions Controller
    /// GET /api/prescriptions?patientId=..., POST /api/prescriptions
    /// </summary>
    [ApiController]
    [Route("api/prescriptions")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? patientId, [FromQuery] string? doctorId)
        {
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                return Ok(await _prescriptionService.GetForPatientAsync(patientId));
            }
            if (!string.IsNullOrWhiteSpace(doctorId))
            {
                return Ok(await _prescriptionService.GetForDoctorAsync(doctorId));
            }
            return BadRequest(new { message = "patientId or doctorId query parameter is required." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);
            return prescription == null
                ? NotFound(new { message = "Prescription not found." })
                : Ok(prescription);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Doctor)]
        public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized(new { message = "Doctor identification failed." });
            }

            try
            {
                var created = await _prescriptionService.CreateAsync(doctorId, dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}