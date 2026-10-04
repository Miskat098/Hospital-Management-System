using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Appointments Management Controller
    /// GET/POST /api/appointments, PUT /api/appointments/{id}/reschedule, POST /api/appointments/{id}/cancel
    /// </summary>
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AppointmentQueryDto query)
        {
            var appointments = await _appointmentService.GetAllAsync(query);
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            return appointment == null
                ? NotFound(new { message = "Appointment not found." })
                : Ok(appointment);
        }

        [HttpGet("availability")]
        [Authorize(Roles = UserRoles.Patient)]
        public async Task<IActionResult> GetAvailability([FromQuery] string doctorId, [FromQuery] DateTime date)
        {
            var availability = await _appointmentService.GetAvailabilityAsync(doctorId, date);
            return Ok(availability);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Patient)]
        public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(patientId))
            {
                return Unauthorized(new { message = "User identification failed." });
            }

            try
            {
                var booked = await _appointmentService.BookAsync(patientId, dto);
                return CreatedAtAction(nameof(GetById), new { id = booked.Id }, booked);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = UserRoles.Doctor + "," + UserRoles.Patient)]
        public async Task<IActionResult> Reschedule(string id, [FromBody] RescheduleAppointmentDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var updated = await _appointmentService.RescheduleAsync(id, dto);
                return updated == null
                    ? NotFound(new { message = "Appointment not found." })
                    : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = UserRoles.Doctor + "," + UserRoles.Patient)]
        public async Task<IActionResult> Cancel(string id, [FromBody] CancelAppointmentDto? dto)
        {
            var cancelled = await _appointmentService.CancelAsync(id, dto?.Reason);
            return cancelled
                ? Ok(new { success = true })
                : NotFound(new { message = "Appointment not found." });
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = UserRoles.Doctor)]
        public async Task<IActionResult> StartVisit(string id)
        {
            var updated = await _appointmentService.StartVisitAsync(id);
            return updated == null
                ? NotFound(new { message = "Appointment not found." })
                : Ok(updated);
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = UserRoles.Doctor)]
        public async Task<IActionResult> Complete(string id)
        {
            var updated = await _appointmentService.CompleteAsync(id);
            return updated == null
                ? NotFound(new { message = "Appointment not found." })
                : Ok(updated);
        }
    }

    /// <summary>
    /// Cancellation Reason Helper DTO
    /// </summary>
    public class CancelAppointmentDto
    {
        public string? Reason { get; set; }
    }
}